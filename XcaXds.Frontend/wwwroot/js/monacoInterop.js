// XcaXds.Frontend/wwwroot/js/monacoInterop.js

function registerHL7(monaco) {
  console.log("Creating HL7 type.");

  // Register the new language
  monaco.languages.register({ id: "hl7" });

  // Define the token provider for HL7
  monaco.languages.setMonarchTokensProvider("hl7", {
    // Set case sensitivity (from Notepad++ setting: caseIgnored="no")
    ignoreCase: false,

    // Define token types
    tokenizer: {
      root: [
        // Segment identifiers (Keywords1 in Notepad++)
        [
          /\b(ACC|ADD|AL1|BHS|BLG|BTS|DG1|DSC|DSP|ERR|EVN|FHS|FT1|FTS|GT1|IN1|IN2|IN3|MFA|MFE|MFI|MRG|MSA|MSH|NCK|NK1|NPU|NSC|NST|NTE|OBR|OBX|ODS|ODT|OM1|OM2|OM3|OM4|OM5|OM6|ORC|PD1|PID|PR1|PRA|PV1|PV2|QRD|QRF|RQ1|RQD|RXA|RXC|RXD|RXE|RXG|RXO|RXR|STF|UB1|UB2|URD|URS|ZAL|ZBN|ZEI|ZLR|ZNI|ZPI|ZQA|ZV1)\b/,
          "segment.identifier",
        ],

        // Message types (Keywords2 in Notepad++)
        [
          /\b(ACK|ADR|ADT|ARD|BAR|DFT|DSR|MCF|MFD|MFK|MFN|MFR|NMD|NMQ|NMR|ORF|ORM|ORR|ORU|OSQ|PGR|QRY|RAR|RAS|RDE|RDR|RDS|RER|RGV|ROR|RRA|RRD|RRE|RRG|UDM|OML|OMS)\b/,
          "message.type",
        ],

        // Event codes (Keywords3 in Notepad++)
        [
          /\b(A01|A02|A03|A04|A05|A06|A07|A08|A09|A10|A11|A12|A13|A14|A15|A16|A17|A18|A19|A20|A21|A22|A23|A24|A25|A26|A27|A28|A29|A30|A31|A32|A33|A34|A35|A36|A37|M01|M02|M03|O01|O02|P01|P02|P03|P04|Q01|Q02|Q03|Q05|R01|R02|R03|R04|O05)\b/,
          "event.code",
        ],

        // Operators (from Notepad++ Operators1)
        [/[\|\^&~\\]/, "operators"],
      ],
    },
  });

  monaco.editor.defineTheme("hl7Theme", {
    base: "vs",
    inherit: true,
    rules: [
      // Following the Notepad++ color scheme:
      { token: "segment.identifier", foreground: "0000FF", fontStyle: "bold" }, // Blue, bold
      { token: "message.type", foreground: "800000", fontStyle: "bold" }, // Dark red, bold
      { token: "event.code", foreground: "800000", fontStyle: "bold" }, // Dark red, bold
      { token: "operators", foreground: "FF0000" }, // Red
    ],
    colors: {},
  });
}

window.monacoInterop = {
  // Map of editor instances
  editors: {},

  // Map of disposables (change handlers, commands)
  disposables: {},

  // Persisted content for each editor
  editorContents: {},
  createHL7Type(monaco) {
    registerHL7(monaco);
  },
  /**
   * Create a Monaco editor instance
   *
   * @param {string}    id          Unique editor ID
   * @param {string}    containerId DOM element ID
   * @param {string}    language    Monaco language mode
   * @param {string}    initialValue Initial text
   * @param {boolean}   readOnly    Read-only?
   * @param {DotNetObjectReference} dotNetRef .NET callbacks
   */
  createEditor(id, containerId, language, initialValue, readOnly, dotNetRef) {
    console.log(`Creating Monaco editor: ${id}`);

    // Proper require.js invocation: modules + callback
    require(["vs/editor/editor.main"], () => {
      try {
        this.doCreateEditor(id, containerId, language, initialValue, readOnly, dotNetRef);
        console.log(`Successfully created Monaco editor: ${id}`);
      } catch (err) {
        console.error(`Error creating Monaco editor: ${id} – ${err.message}`);
      }
    }, (err) => {
      console.error(`Require.js failed to load Monaco: ${err}`);
    });
  },

  /**
   * Internal: actually instantiate the editor
   */
  doCreateEditor(id, containerId, language, initialValue, readOnly, dotNetRef) {
    // If it already exists, dispose old instance
    if (this.editors[id]) {
      this.disposeEditor(id);
    }

    const container = document.getElementById(containerId);
    if (!container) {
      throw new Error(`Editor container "${containerId}" not found`);
    }

    // Restore persisted content or use initialValue
    const content = this.editorContents[id] ?? initialValue ?? "";
    this.createHL7Type(monaco);

    console.log(`Creating Monaco editor: ${id} (${language})`);

    // Create Monaco
    const editor = monaco.editor.create(container, {
      value: content,
      language,
      automaticLayout: true,
      minimap: { enabled: false },
      scrollBeyondLastLine: false,
      theme: language === "hl7" ? "hl7Theme" : "vs",
      readOnly,
      wordWrap: "on",
    });

    // Track disposables
    this.disposables[id] = [];

    // Hook up content-change → Blazor
    if (dotNetRef) {
      const changeDisp = editor.onDidChangeModelContent(() => {
        this.editorContents[id] = editor.getValue();
        dotNetRef.invokeMethodAsync("HandleContentChanged", this.editorContents[id]);
      });
      this.disposables[id].push(changeDisp);

      // Hook Ctrl+S → Blazor save
      const saveDisp = editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () =>
        dotNetRef.invokeMethodAsync("HandleSave")
      );
      this.disposables[id].push(saveDisp);
    }

    this.editors[id] = editor;

    // Global resize handler (only once)
    if (!window._monacoInteropHasResizeHandler) {
      window._monacoInteropResizeHandler = () => {
        for (const eid in window.monacoInterop.editors) {
          window.monacoInterop.editors[eid]?.layout();
        }
      };
      window.addEventListener("resize", window._monacoInteropResizeHandler);
      window._monacoInteropHasResizeHandler = true;
    }
  },

  /** Return current text */
  getValue(id) {
    const ed = this.editors[id];
    if (ed) {
      try {
        return ed.getValue();
      } catch {}
    }
    return this.editorContents[id] ?? "";
  },

  /** Set text programmatically */
  setValue(id, value) {
    this.editorContents[id] = value;
    this.editors[id]?.setValue(value);
  },

  /** Force a layout pass */
  refreshLayout(id) {
    this.editors[id]?.layout();
  },

  /** Recreate after a Blazor hot-reload */
  recreateEditor(id, containerId, language, dotNetRef) {
    const content = this.editorContents[id] ?? "";
    this.createEditor(id, containerId, language, content, false, dotNetRef);
  },

  /** Dispose the editor and clean up listeners */
  disposeEditor(id) {
    const editor = this.editors[id];
    if (!editor) return;

    // Persist content
    this.editorContents[id] = editor.getValue();

    // Dispose editor
    editor.dispose();

    // Dispose our Monaco disposables
    (this.disposables[id] || []).forEach((d) => d.dispose?.());
    delete this.disposables[id];

    delete this.editors[id];
    console.log(`Editor "${id}" disposed`);

    // If no editors remain, remove the global resize listener
    if (!Object.keys(this.editors).length && window._monacoInteropHasResizeHandler) {
      window.removeEventListener("resize", window._monacoInteropResizeHandler);
      window._monacoInteropHasResizeHandler = false;
      delete window._monacoInteropResizeHandler;
    }
  },
};
