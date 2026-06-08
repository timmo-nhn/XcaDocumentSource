using XcaXds.Terminology.Models.Custom;

namespace XcaXds.Terminology.ValueSets;

public static class Constants
{     
    // Old static values
    public static class KjForskriftCategoryCodes
    {
        public const string EpikriserOgSammenfatninger = "A00-1";
        public const string KontinuerligLopendeJournal = "B00-1";
        public const string ProvesvarVevOgVaesker = "C00-1";
        public const string Organfunksjon = "D00-1";
        public const string BildediagnostikkOgAndreMedisinskeBilder = "E00-1";
        public const string KurveObservasjonOgBehandling = "F00-1";
        public const string Korrespondanse = "I00-1";
        public const string AttesterMeldingOgErklaeringer = "J00-1";
        public const string TestOgScoring = "S00-1";
    }


    public static class Volven
    {
        public static class Gender_3101
        {
            public const string System = "2.16.578.1.12.4.1.1.3101";

            ///<summary>Ikke kjent</summary>
            public const string Unknown = "0";
            ///<summary>Mann</summary>
            public const string Male = "1";
            ///<summary>Kvinne</summary>
            public const string Female = "2";
            ///<summary>Ikke spesifisert</summary>
            public const string Unspecified = "9";
        }

        public static class EventCode_7010
        {
            public const string System = "2.16.578.1.12.4.1.1.7010";
        }

        public static class EventCode_7210
        {
            public const string System = "2.16.578.1.12.4.1.1.7210";
        }

        public static class EventCode_7220
        {
            public const string System = "2.16.578.1.12.4.1.1.7220";
        }

        public static class EventCode_7270
        {
            public const string System = "2.16.578.1.12.4.1.1.7270";
        }

        public static class FacilityType_1303
        {
            public const string System = "2.16.578.1.12.4.1.1.1303";

            /// <summary>Alminnelige somatiske sykehus</summary>
            public const string _86_101 = "86.101";
            /// <summary>Somatiske spesialsykehus</summary>
            public const string _86_102 = "86.102";
            /// <summary>Andre somatiske spesialinstitusjoner</summary>
            public const string _86_103 = "86.103";
            /// <summary>Institusjoner i psykisk helsevern for voksne</summary>
            public const string _86_104 = "86.104";
            /// <summary>Institusjoner i psykisk helsevern for barn og unge</summary>
            public const string _86_105 = "86.105";
            /// <summary>Rusmiddelinstitusjoner</summary>
            public const string _86_106 = "86.106";
            /// <summary>Rehabiliterings- og opptreningsinstitusjoner</summary>
            public const string _86_107 = "86.107";
            /// <summary>Allmenn legetjeneste</summary>
            public const string _86_211 = "86.211";
            /// <summary>Somatiske poliklinikker</summary>
            public const string _86_212 = "86.212";
            /// <summary>Spesialisert legetjeneste, unntatt psykiatrisk legetjeneste</summary>
            public const string _86_221 = "86.221";
            /// <summary>Legetjenester innen psykisk helsevern</summary>
            public const string _86_222 = "86.222";
            /// <summary>Poliklinikker i psykisk helsevern for voksne</summary>
            public const string _86_223 = "86.223";
            /// <summary>Poliklinikker i psykisk helsevern for barn og unge</summary>
            public const string _86_224 = "86.224";
            /// <summary>Rusmiddelpoliklinikker</summary>
            public const string _86_225 = "86.225";
            /// <summary>Tannhelsetjenester</summary>
            public const string _86_230 = "86.230";
            /// <summary>Hjemmesykepleie</summary>
            public const string _86_901 = "86.901";
            /// <summary>Fysioterapitjeneste</summary>
            public const string _86_902 = "86.902";
            /// <summary>Helsestasjons- og skolehelsetjeneste</summary>
            public const string _86_903 = "86.903";
            /// <summary>Annen forebyggende helsetjeneste</summary>
            public const string _86_904 = "86.904";
            /// <summary>Klinisk psykologtjeneste</summary>
            public const string _86_905 = "86.905";
            /// <summary>Medisinske laboratorietjenester</summary>
            public const string _86_906 = "86.906";
            /// <summary>Ambulansetjenester</summary>
            public const string _86_907 = "86.907";
            /// <summary>Andre helsetjenester</summary>
            public const string _86_909 = "86.909";
            /// <summary>Somatiske spesialsykehjem</summary>
            public const string _87_101 = "87.101";
            /// <summary>Somatiske sykehjem</summary>
            public const string _87_102 = "87.102";
            /// <summary>Psykiatriske sykehjem</summary>
            public const string _87_201 = "87.201";
            /// <summary>Omsorgsinstitusjoner for rusmiddelmisbrukere</summary>
            public const string _87_202 = "87.202";
            /// <summary>Bofellesskap for psykisk utviklingshemmede</summary>
            public const string _87_203 = "87.203";
            /// <summary>Aldershjem</summary>
            public const string _87_301 = "87.301";
            /// <summary>Bofellesskap for eldre og funksjonshemmede med fast tilknyttet personell hele døgnet</summary>
            public const string _87_302 = "87.302";
            /// <summary>Bofellesskap for eldre og funksjonshemmede med fast tilknyttet personell deler av døgnet</summary>
            public const string _87_303 = "87.303";
            /// <summary>Avlastningsboliger/-institusjoner</summary>
            public const string _87_304 = "87.304";
            /// <summary>Barneboliger</summary>
            public const string _87_305 = "87.305";
            /// <summary>Institusjoner innen barne- og ungdomsvern</summary>
            public const string _87_901 = "87.901";
            /// <summary>Omsorgsinstitusjoner ellers</summary>
            public const string _87_909 = "87.909";
            /// <summary>Hjemmehjelp</summary>
            public const string _88_101 = "88.101";
            /// <summary>Dagsentra/aktivitetssentra for eldre og funksjonshemmede</summary>
            public const string _88_102 = "88.102";
            /// <summary>Eldresentre</summary>
            public const string _88_103 = "88.103";
            /// <summary>Barnehager</summary>
            public const string _88_911 = "88.911";
            /// <summary>Barneparker og dagmammaer</summary>
            public const string _88_912 = "88.912";
            /// <summary>Skolefritidsordninger</summary>
            public const string _88_913 = "88.913";
            /// <summary>Fritidsklubber for barn og ungdom</summary>
            public const string _88_914 = "88.914";
            /// <summary>Barneverntjenester</summary>
            public const string _88_991 = "88.991";
            /// <summary>Familieverntjenester</summary>
            public const string _88_992 = "88.992";
            /// <summary>Arbeidstrening for ordinært arbeidsmarked</summary>
            public const string _88_993 = "88.993";
            /// <summary>Varig tilrettelagt arbeid</summary>
            public const string _88_994 = "88.994";
            /// <summary>Sosiale velferdsorganisasjoner</summary>
            public const string _88_995 = "88.995";
            /// <summary>Asylmottak</summary>
            public const string _88_996 = "88.996";
            /// <summary>Sosialtjenester for rusmiddelmisbrukere uten botilbud</summary>
            public const string _88_997 = "88.997";
            /// <summary>Kommunale sosialkontortjenester</summary>
            public const string _88_998 = "88.998";
            /// <summary>Andre sosialtjenester uten botilbud</summary>
            public const string _88_999 = "88.999";
        }

        public static class FacilityType_1305
        {
            public const string System = "2.16.578.1.12.4.1.1.1305";

            ///<summary>Somatiske sykehustjenester</summary>
            public const string _86_101 = "86.101";
            ///<summary>Psykiatriske sykehustjenester for voksne</summary>
            public const string _86_102 = "86.102";
            ///<summary>Spesialisert rusbehandling</summary>
            public const string _86_103 = "86.103";
            ///<summary>Psykiatriske sykehustjenester for barn og ungdom</summary>
            public const string _86_104 = "86.104";
            ///<summary>Allmennlegetjenester</summary>
            public const string _86_210 = "86.210";
            ///<summary>Spesialiserte legetjenester, unntatt psykiatriske legetjenester</summary>
            public const string _86_221 = "86.221";
            ///<summary>Psykiatriske legetjenester</summary>
            public const string _86_222 = "86.222";
            ///<summary>Tannlegetjenester</summary>
            public const string _86_230 = "86.230";
            ///<summary>Medisinske laboratorietjenester og bildediagnostikk</summary>
            public const string _86_910 = "86.910";
            ///<summary>Ambulansetransport unntatt luftambulanse</summary>
            public const string _86_921 = "86.921";
            ///<summary>Luftambulansetransport med fly eller helikopter</summary>
            public const string _86_922 = "86.922";
            ///<summary>Psykolog- og psykoterapitjenester, unntatt legetjenester innenfor psykiatri</summary>
            public const string _86_930 = "86.930";
            ///<summary>Sykepleietjenester og andre helsetjenester i hjemmet</summary>
            public const string _86_941 = "86.941";
            ///<summary>Helsestasjons- og skolehelsetjenester</summary>
            public const string _86_942 = "86.942";
            ///<summary>Fysioterapi- og ergoterapitjenester</summary>
            public const string _86_950 = "86.950";
            ///<summary>Aktiviteter innenfor tradisjonell, komplementær og alternativ medisin</summary>
            public const string _86_960 = "86.960";
            ///<summary>Formidlingstjenester tilknyttet lege-, tannlegetjenester og andre helsetjenester</summary>
            public const string _86_970 = "86.970";
            ///<summary>Ortopedi- og fotterapitjenester</summary>
            public const string _86_991 = "86.991";
            ///<summary>Forebyggende helsearbeid</summary>
            public const string _86_992 = "86.992";
            ///<summary>Andre helsetjenester ellers</summary>
            public const string _86_993 = "86.993";
            ///<summary>Tjenester i spesialinstitusjon innenfor helse- og omsorg</summary>
            public const string _87_101 = "87.101";
            ///<summary>Sykehjemstjenester</summary>
            public const string _87_102 = "87.102";
            ///<summary>Øyeblikkelig hjelp døgntilbud</summary>
            public const string _87_103 = "87.103";
            ///<summary>Tjenester i avlastningsboliger/-institusjoner</summary>
            public const string _87_104 = "87.104";
            ///<summary>Tjenester i barneboliger</summary>
            public const string _87_105 = "87.105";
            ///<summary>Helse- og omsorgstjenester i bofelleskap, samlokaliserte omsorgsboliger og lignende</summary>
            public const string _87_106 = "87.106";
            ///<summary>Omsorgstjenester i botilbud for personer med psykiske helseproblemer og/eller rusmiddelproblemer</summary>
            public const string _87_201 = "87.201";
            ///<summary>Omsorgstjenester i botilbud for personer med utviklingshemming eller med tilsvarende funksjonsnedsettelse</summary>
            public const string _87_202 = "87.202";
            ///<summary>Omsorgstjenester i botilbud for eldre eller personer med fysisk funksjonsnedsettelse</summary>
            public const string _87_300 = "87.300";
            ///<summary>Formidlingstjenester tilknyttet omsorgstjenester i botilbud</summary>
            public const string _87_910 = "87.910";
            ///<summary>Tjenester i barneverninstitusjoner</summary>
            public const string _87_991 = "87.991";
            ///<summary>Asylmottakstjenester</summary>
            public const string _87_992 = "87.992";
            ///<summary>Andre botilbud innenfor sosiale tjenester ikke nevnt annet sted</summary>
            public const string _87_999 = "87.999";
            ///<summary>Praktisk bistand i hjemmet</summary>
            public const string _88_101 = "88.101";
            ///<summary>Dagaktivitetstilbud tilpasset målgrupper</summary>
            public const string _88_102 = "88.102";
            ///<summary>Tjenester i seniorsentre</summary>
            public const string _88_103 = "88.103";
            ///<summary>Brukerstyrt personlig assistanse (BPA)</summary>
            public const string _88_104 = "88.104";
            ///<summary>Avlastning utenfor institusjon</summary>
            public const string _88_105 = "88.105";
            ///<summary>Støttekontakt- og besøkstjeneste</summary>
            public const string _88_106 = "88.106";
            ///<summary>Dagaktivitetstilbud for barn</summary>
            public const string _88_910 = "88.910";
            ///<summary>Barneverntjenester</summary>
            public const string _88_991 = "88.991";
            ///<summary>Familieverntjenester</summary>
            public const string _88_992 = "88.992";
            ///<summary>Arbeidstrening og varig tilrettelagt arbeid</summary>
            public const string _88_993 = "88.993";
            ///<summary>Velferdstjenester til sårbare grupper</summary>
            public const string _88_994 = "88.994";
            ///<summary>Sosialkontortjenester</summary>
            public const string _88_995 = "88.995";
            ///<summary>Andre sosialtjenester uten botilbud ellers</summary>
            public const string _88_996 = "88.996";

        }

        public static class PracticeSetting_8651
        {
            public const string System = "2.16.578.1.12.4.1.1.8651";

            ///<summary>Operasjon</summary>
            public const string A01 = "A01";
            ///<summary>Observasjonsenhet</summary>
            public const string A02 = "A02";
            ///<summary>Intensivenhet</summary>
            public const string A03 = "A03";
            ///<summary>Overvåkningsenhet</summary>
            public const string A04 = "A04";
            ///<summary>Intermediærenhet</summary>
            public const string A05 = "A05";
        }

        public static class PracticeSetting_8653
        {
            public const string System = "2.16.578.1.12.4.1.1.8653";

            /// <summary>Pleietjenester</summary>
            public const string _1 = "1";
            /// <summary>Pasienthotell</summary>
            public const string _2 = "2";
            /// <summary>Pasientmottak, elektiv</summary>
            public const string _3 = "3";
            /// <summary>Legevakt</summary>
            public const string _4 = "4";
            /// <summary>Vurdering av henvisning</summary>
            public const string _5 = "5";
            /// <summary>Akuttmottak	</summary>
            public const string _6 = "6";
            /// <summary>Ambulansetjeneste, ordinær</summary>
            public const string _7 = "7";
            /// <summary>Luftambulanse</summary>
            public const string _8 = "8";
            /// <summary>AMK-sentral</summary>
            public const string _9 = "9";
        }

        public static class PracticeSetting_8654
        {
            public const string System = "2.16.578.1.12.4.1.1.8654";

            /// <summary>Bildediagnostikk</summary>
            public const string B = "B";
            /// <summary>Røntgen</summary>
            public const string B01 = "B01";
            /// <summary>Ultralyd</summary>
            public const string B02 = "B02";
            /// <summary>Angiografi</summary>
            public const string B03 = "B03";
            /// <summary>Tomografi MR</summary>
            public const string B04 = "B04";
            /// <summary>Tomografi CT</summary>
            public const string B05 = "B05";
            /// <summary>Nukleærmedisin</summary>
            public const string B06 = "B06";
            /// <summary>Nevroradiologi</summary>
            public const string B07 = "B07";
            /// <summary>Intervensjonsradiologi</summary>
            public const string B08 = "B08";
            /// <summary>Laboratoriefag</summary>
            public const string L = "L";
            /// <summary>Klinisk farmakologi</summary>
            public const string L01 = "L01";
            /// <summary>Immunologi, allergologi og transfusjonsmedisin</summary>
            public const string L02 = "L02";
            /// <summary>Immunologi og allergologi</summary>
            public const string L0201 = "L0201";
            /// <summary>Transfusjonsmedisin</summary>
            public const string L0202 = "L0202";
            /// <summary>Medisinsk biokjemi</summary>
            public const string L03 = "L03";
            /// <summary>Medisinsk mikrobiologi</summary>
            public const string L04 = "L04";
            /// <summary>Patologi</summary>
            public const string L06 = "L06";
            /// <summary>Klinisk nevrofysiologi</summary>
            public const string L07 = "L07";
            /// <summary>Nevrovaskulært laboratorium</summary>
            public const string L08 = "L08";
            /// <summary>Nevroimmunologisk laboratorium</summary>
            public const string L09 = "L09";
            /// <summary>Cytogenetikk og molekylærgenetikk</summary>
            public const string L10 = "L10";

        }

        public static class PracticeSetting_8655
        {
            public const string System = "2.16.578.1.12.4.1.1.8655";

            /// <summmary>Andre helsehjelpsområder</summary>
            public const string A = "A";
            /// <summmary>Sosionomtjenester</summary>
            public const string A01 = "A01";
            /// <summmary>Ergoterapi</summary>
            public const string A02 = "A02";
            /// <summmary>Fysioterapi</summary>
            public const string A03 = "A03";
            /// <summmary>Kiropraktikk</summary>
            public const string A04 = "A04";
            /// <summmary>Ernæringsfysiologi</summary>
            public const string A05 = "A05";
            /// <summmary>Tannhelse</summary>
            public const string A06 = "A06";
            /// <summmary>Audiografi</summary>
            public const string A07 = "A07";
            /// <summmary>Spesialpedagogikk</summary>
            public const string A08 = "A08";
            /// <summmary>Logopedi</summary>
            public const string A09 = "A09";
            /// <summmary>Farmasi</summary>
            public const string A10 = "A10";
            /// <summmary>Yrkes- og arbeidsmedisin</summary>
            public const string A11 = "A11";
            /// <summmary>Psykologtjeneste</summary>
            public const string A12 = "A12";
            /// <summmary>Helsehjelp knyttet til habilitering og rehabilitering</summary>
            public const string H = "H";
            /// <summmary>Barnehabilitering</summary>
            public const string H07 = "H07";
            /// <summmary>Voksenhabilitering</summary>
            public const string H08 = "H08";
            /// <summmary>Rehabilitering</summary>
            public const string H09 = "H09";
            /// <summmary>Psykisk helsevern</summary>
            public const string P = "P";
            /// <summmary>Psykisk helsevern for barn og unge (BUP)</summary>
            public const string PB = "PB";
            /// <summmary>Familieterapi</summary>
            public const string PB01 = "PB01";
            /// <summmary>Spiseforstyrrelser hos barn</summary>
            public const string PB02 = "PB02";
            /// <summmary>Psykisk helsevern for voksne</summary>
            public const string PV = "PV";
            /// <summmary>Spiseforstyrrelser hos voksne</summary>
            public const string PV01 = "PV01";
            /// <summmary>Psykiatrisk helsehjelp til døve</summary>
            public const string PV02 = "PV02";
            /// <summmary>Unge schizofrene</summary>
            public const string PV03 = "PV03";
            /// <summmary>Alderspsykiatrisk behandling</summary>
            public const string PV04 = "PV04";
            /// <summmary>Psykiatrisk helsehjelp til asylsøkere og flyktninger</summary>
            public const string PV05 = "PV05";
            /// <summmary>Tidlig intervensjon</summary>
            public const string PV06 = "PV06";
            /// <summmary>Pasienter med langvarig funksjonssvikt</summary>
            public const string PV07 = "PV07";
            /// <summmary>Førstegangspsykose</summary>
            public const string PV08 = "PV08";
            /// <summmary>Habilitering/Rehabilitering (psykisk helsevern for voksne)</summary>
            public const string PV09 = "PV09";
            /// <summmary>Familieterapi/behandling</summary>
            public const string PV10 = "PV10";
            /// <summmary>Sikkerhetspsykiatri</summary>
            public const string PV11 = "PV11";
            /// <summmary>Helsehjelp knyttet til rusmiddelavhengighet og annen avhengighet</summary>
            public const string R = "R";
            /// <summmary>Spilleavhengighet og annen avhengighet</summary>
            public const string R01 = "R01";
            /// <summmary>Rusmiddelavhengighet med alvorlig psykiatrisk sykdom (dobbeldiagnose)</summary>
            public const string R02 = "R02";
            /// <summmary>Rusmiddelavhengighet med langvarig funksjonssvikt</summary>
            public const string R03 = "R03";
            /// <summmary>Førstegangspsykose knyttet til rusmiddelavhengighet</summary>
            public const string R04 = "R04";
            /// <summmary>Utredning av rusmiddelavhengighet eller annen avhengighet</summary>
            public const string R05 = "R05";
            /// <summmary>Avrusning/ avgiftning/ stabilisering</summary>
            public const string R06 = "R06";
            /// <summmary>Familieterapi, parterapi og pårørendeterapi</summary>
            public const string R07 = "R07";
            /// <summmary>Legemiddelassistert rehabilitering (LAR)</summary>
            public const string R08 = "R08";
            /// <summmary>Terapeutisk samfunn, kollektiv osv.</summary>
            public const string R09 = "R09";
            /// <summmary>Innsatte under paragraf 12-soning</summary>
            public const string R10 = "R10";
            /// <summmary>Tverrfaglig spesialisert behandling av rusmiddelmisbruk</summary>
            public const string R11 = "R11";
            /// <summmary>Helsehjelp knyttet til somatisk sykdom</summary>
            public const string S = "S";
            /// <summmary>Allmennmedisin</summary>
            public const string S01 = "S01";
            /// <summmary>Kirurgi</summary>
            public const string S02 = "S02";
            /// <summmary>Generell kirurgi</summary>
            public const string S0201 = "S0201";
            /// <summmary>Barnekirurgi</summary>
            public const string S0202 = "S0202";
            /// <summmary>Bryst og endokrin kirurgi</summary>
            public const string S0203 = "S0203";
            /// <summmary>Gastroenterologisk kirurgi</summary>
            public const string S0204 = "S0204";
            /// <summmary>Karkirurgi</summary>
            public const string S0205 = "S0205";
            /// <summmary>Kjeve- og ansiktskirurgi</summary>
            public const string S0206 = "S0206";
            /// <summmary>Nevrokirurgi</summary>
            public const string S0207 = "S0207";
            /// <summmary>Ortopedisk kirurgi</summary>
            public const string S0208 = "S0208";
            /// <summmary>Plastikkirurgi</summary>
            public const string S0209 = "S0209";
            /// <summmary>Thoraxkirurgi</summary>
            public const string S0210 = "S0210";
            /// <summmary>Urologi</summary>
            public const string S0211 = "S0211";
            /// <summmary>Indremedisin</summary>
            public const string S03 = "S03";
            /// <summmary>Endokrinologi</summary>
            public const string S0301 = "S0301";
            /// <summmary>Fordøyelsessykdommer</summary>
            public const string S0302 = "S0302";
            /// <summmary>Geriatri</summary>
            public const string S0303 = "S0303";
            /// <summmary>Blodsykdommer</summary>
            public const string S0304 = "S0304";
            /// <summmary>Infeksjonsmedisin</summary>
            public const string S0305 = "S0305";
            /// <summmary>Hjertesykdommer</summary>
            public const string S0306 = "S0306";
            /// <summmary>Hjerterytmeforstyrrelser</summary>
            public const string S030601 = "S030601";
            /// <summmary>Ekkokardiografi og bildediagnostikk</summary>
            public const string S030602 = "S030602";
            /// <summmary>Klinisk kardiologi</summary>
            public const string S030603 = "S030603";
            /// <summmary>Forebyggende kardiologi</summary>
            public const string S030604 = "S030604";
            /// <summmary>Invasiv kardiologi</summary>
            public const string S030605 = "S030605";
            /// <summmary>Lungesykdommer</summary>
            public const string S0307 = "S0307";
            /// <summmary>Nyresykdommer</summary>
            public const string S0308 = "S0308";
            /// <summmary>Dialyse</summary>
            public const string S0309 = "S0309";
            /// <summmary>Fødselshjelp og kvinnesykdommer</summary>
            public const string S04 = "S04";
            /// <summmary>Generell gynekologi</summary>
            public const string S0401 = "S0401";
            /// <summmary>Gynekologisk onkologi</summary>
            public const string S0402 = "S0402";
            /// <summmary>Obstetrikk</summary>
            public const string S0403 = "S0403";
            /// <summmary>Assistert befruktning</summary>
            public const string S0404 = "S0404";
            /// <summmary>Fostermedisin</summary>
            public const string S0405 = "S0405";
            /// <summmary>Hud- og veneriske sykdommer</summary>
            public const string S05 = "S05";
            /// <summmary>Hudsykdommer</summary>
            public const string S0501 = "S0501";
            /// <summmary>Veneriske sykdommer</summary>
            public const string S0502 = "S0502";
            /// <summmary>Barnesykdommer</summary>
            public const string S06 = "S06";
            /// <summmary>Nyfødtmedisin</summary>
            public const string S0601 = "S0601";
            /// <summmary>Intensivbehandling av barn</summary>
            public const string S0602 = "S0602";
            /// <summmary>Nevrologi</summary>
            public const string S07 = "S07";
            /// <summmary>Generell nevrologi</summary>
            public const string S0701 = "S0701";
            /// <summmary>Cerebrovaskulære sykdommer</summary>
            public const string S0702 = "S0702";
            /// <summmary>Epilepsi</summary>
            public const string S0703 = "S0703";
            /// <summmary>Nevrofysiologi</summary>
            public const string S0704 = "S0704";
            /// <summmary>Anestesiologi/smertebehandling</summary>
            public const string S08 = "S08";
            /// <summmary>Øre-nese-halssykdommer</summary>
            public const string S09 = "S09";
            /// <summmary>Audiologi</summary>
            public const string S0901 = "S0901";
            /// <summmary>Laryngologi/Foniatri</summary>
            public const string S0902 = "S0902";
            /// <summmary>Balansemedisin</summary>
            public const string S0903 = "S0903";
            /// <summmary>Søvnrelaterte sykdommer</summary>
            public const string S0904 = "S0904";
            /// <summmary>Nese- og bihulesykdommer</summary>
            public const string S0905 = "S0905";
            /// <summmary>Otologi</summary>
            public const string S0906 = "S0906";
            /// <summmary>Hode- og halskirurgi</summary>
            public const string S0907 = "S0907";
            /// <summmary>Allergologi</summary>
            public const string S0908 = "S0908";
            /// <summmary>Pediatriske øre-nese-halssykdommer</summary>
            public const string S0909 = "S0909";
            /// <summmary>Øyesykdommer</summary>
            public const string S10 = "S10";
            /// <summmary>Onkologi</summary>
            public const string S11 = "S11";
            /// <summmary>Sarkomer</summary>
            public const string S1101 = "S1101";
            /// <summmary>Revmatologi</summary>
            public const string S12 = "S12";
            /// <summmary>Tverrfaglig ryggbehandling</summary>
            public const string S13 = "S13";
            /// <summmary>Palliativ medisin</summary>
            public const string S14 = "S14";
            /// <summmary>Medisinsk genetikk</summary>
            public const string S15 = "S15";
            /// <summmary>Fysikalsk medisin og rehabilitering</summary>
            public const string S16 = "S16";
        }

        public static class PracticeSetting_8663
        {
            public const string System = "2.16.578.1.12.4.1.1.8663";

            /// <summary>Legevakt</summary>
            public const string KA02 = "KA02";
            /// <summary>Kommuneoverlege</summary>
            public const string KA03 = "KA03";
            /// <summary>Smittevern</summary>
            public const string KA0301 = "KA0301";
            /// <summary>Migrasjonshelse</summary>
            public const string KA04 = "KA04";
            /// <summary>Kommunal nettlege</summary>
            public const string KA05 = "KA05";
            /// <summary>Sosialtjeneste</summary>
            public const string KD01 = "KD01";
            /// <summary>Saksbehandling</summary>
            public const string KD0501 = "KD0501";
            /// <summary>Helsestasjons- og skolehelsetjeneste</summary>
            public const string KF01 = "KF01";
            /// <summary>Helsestasjon for ungdom</summary>
            public const string KF0103 = "KF0103";
            /// <summary>Legetjeneste ved sykehjem mv.</summary>
            public const string KP01 = "KP01";
            /// <summary>Sykepleietjeneste</summary>
            public const string KP02 = "KP02";
            /// <summary>Fengselshelsetjeneste</summary>
            public const string KX01 = "KX01";
            /// <summary>Frisklivssentral</summary>
            public const string KX04 = "KX04";
            /// <summary>Øyeblikkelig hjelp døgntilbud (ØHD)</summary>
            public const string KX05 = "KX05";
            /// <summary>Kreftkoordinator</summary>
            public const string KX06 = "KX06";
            /// <summary>Demenskoordinator</summary>
            public const string KX07 = "KX07";
            /// <summary>Familieteam</summary>
            public const string KX12 = "KX12";
            /// <summary>Barnevern</summary>
            public const string KX15 = "KX15";
            /// <summary>Pedagogisk-psykologisk tjeneste (PPT)</summary>
            public const string KX16 = "KX16";
            /// <summary>Barnevernvakt</summary>
            public const string KX18 = "KX18";
        }

        public static class TypeCode_9602
        {
            public const string System = "2.16.578.1.12.4.1.1.9602";

            /// <summary>Kriseplan</summary>
            public const string A01_2 = "A01-2";
            /// <summary>Individuell plan</summary>
            public const string A02_2 = "A02-2";
            /// <summary>Epikrise</summary>
            public const string A03_2 = "A03-2";
            /// <summary>Sykepleiesammenfatning</summary>
            public const string A04_2 = "A04-2";
            /// <summary>Fysioterapisammenfatning</summary>
            public const string A05_2 = "A05-2";
            /// <summary>Ergoterapisammenfatning</summary>
            public const string A06_2 = "A06-2";
            /// <summary>Psykologsammenfatning</summary>
            public const string A07_2 = "A07-2";
            /// <summary>Sosionomsammenfatning</summary>
            public const string A08_2 = "A08-2";
            /// <summary>Ernæringsfysiologsammenfatning</summary>
            public const string A09_2 = "A09-2";
            /// <summary>Annet fagpersonell sammenfatning</summary>
            public const string A10_2 = "A10-2";
            /// <summary>Tverrfaglig sammenfatning</summary>
            public const string A11_2 = "A11-2";
            /// <summary>Utskrivings-/Pasientorientering</summary>
            public const string A12_2 = "A12-2";
            /// <summary>Poliklinisk epikrise</summary>
            public const string A13_2 = "A13-2";

            /// <summary>Tverrfaglig behandlingsplan</summary>
            public const string B01_2 = "B01-2";
            /// <summary>Journalnotat</summary>
            public const string B02_2 = "B02-2";
            /// <summary>Poliklinisk notat</summary>
            public const string B03_2 = "B03-2";

            /// <summary>Medisinsk biokjemi</summary>
            public const string C01_2 = "C01-2";
            /// <summary>Blodbank og immunologi</summary>
            public const string C02_2 = "C02-2";
            /// <summary>Mikrobiologi, virologi og serologi</summary>
            public const string C03_2 = "C03-2";
            /// <summary>Patologi, histologi og cytologi</summary>
            public const string C04_2 = "C04-2";
            /// <summary>Klinisk farmakologi</summary>
            public const string C05_2 = "C05-2";
            /// <summary>Medisinsk genetikk</summary>
            public const string C06_2 = "C06-2";
            /// <summary>Allergiutredning</summary>
            public const string C07_2 = "C07-2";

            /// <summary>Hjerte og kretsløp</summary>
            public const string D01_2 = "D01-2";
            /// <summary>Lunge</summary>
            public const string D02_2 = "D02-2";
            /// <summary>Fordøyelse</summary>
            public const string D03_2 = "D03-2";
            /// <summary>Urinveier</summary>
            public const string D04_2 = "D04-2";
            /// <summary>Gyn/Reproduksjon</summary>
            public const string D05_2 = "D05-2";
            /// <summary>Nervesystemet</summary>
            public const string D06_2 = "D06-2";
            /// <summary>Ledd/ ben/ skjelett</summary>
            public const string D07_2 = "D07-2";
            /// <summary>ØNH</summary>
            public const string D08_2 = "D08-2";
            /// <summary>Øye</summary>
            public const string D09_2 = "D09-2";
            /// <summary>Hud</summary>
            public const string D10_2 = "D10-2";
            /// <summary>Endokrinologi</summary>
            public const string D11_2 = "D11-2";
            /// <summary>Metabolisme</summary>
            public const string D12_2 = "D12-2";
            /// <summary>Beinmargsutstryk</summary>
            public const string D13_2 = "D13-2";

            /// <summary>Bildediagnostiske svar</summary>
            public const string E01_2 = "E01-2";
            /// <summary>Foto og film</summary>
            public const string E02_2 = "E02-2";

            /// <summary>Kurve</summary>
            public const string F01_2 = "F01-2";
            /// <summary>Anestesi- og opr. Rapporter</summary>
            public const string F02_2 = "F02-2";
            /// <summary>Intensiv/postoperativ observasjon</summary>
            public const string F03_2 = "F03-2";
            /// <summary>Svangerskap og fødsel</summary>
            public const string F04_2 = "F04-2";
            /// <summary>Diabetes/ endokrinologi</summary>
            public const string F05_2 = "F05-2";
            /// <summary>Onkologi/ hematologi</summary>
            public const string F06_2 = "F06-2";
            /// <summary>Nyre/ dialyse</summary>
            public const string F07_2 = "F07-2";
            /// <summary>Smertebehandling</summary>
            public const string F08_2 = "F08-2";
            /// <summary>Ambulansejournal</summary>
            public const string F09_2 = "F09-2";
            /// <summary>Transplantasjon</summary>
            public const string F10_2 = "F10-2";

            /// <summary>Henvisninger</summary>
            public const string I01_2 = "I01-2";
            /// <summary>Brev</summary>
            public const string I02_2 = "I02-2";

            /// <summary>Sykmeldinger og trygdesaker</summary>
            public const string J01_2 = "J01-2";
            /// <summary>Legeerklæring om dødsfall</summary>
            public const string J02_2 = "J02-2";

            /// <summary>Tester</summary>
            public const string S01_2 = "S01-2";
            /// <summary>Systematiserte diagnostiske intervju</summary>
            public const string S02_2 = "S02-2";
            /// <summary>Voldsrisikovurdering</summary>
            public const string S03_2 = "S03-2";
        }

        public static class CategoryCode_9602
        {
            public const string System = "2.16.578.1.12.4.1.1.9602";

            /// <summary>Epikriser og sammenfatninger</summary>
            public const string A00_1 = "A00-1";

            /// <summary>Kontinuerlig/løpende journal</summary>
            public const string B00_1 = "B00-1";

            /// <summary>Prøvesvar, vev og væsker</summary>
            public const string C00_1 = "C00-1";

            /// <summary>Organfunksjon</summary>
            public const string D00_1 = "D00-1";

            /// <summary>Bildediagnostikk</summary>
            public const string E00_1 = "E00-1";

            /// <summary>Kurve, observasjon og behandling</summary>
            public const string F00_1 = "F00-1";

            /// <summary>Korrespondanse</summary>
            public const string I00_1 = "I00-1";

            /// <summary>Attester, melding og erklæringer</summary>
            public const string J00_1 = "J00-1";

            /// <summary>Test og scoring</summary>
            public const string S00_1 = "S00-1";
        }

        public static class ConfidentialityCode_9603
        {
            public const string System = "2.16.578.1.12.4.1.1.9603";

            /// <summary> Normal</summary>
            public const string N = "N";
            /// <summary> Nektet, andre grunner</summary>
            public const string NORN_ANG = "NORN_ANG";
            /// <summary> Nektet, alle dokumenter</summary>
            public const string NORN_ALL = "NORN_ALL";
            /// <summary> Nektet, duplikat</summary>
            public const string NORN_DUP = "NORN_DUP";
            /// <summary> Nektet, eget ønske</summary>
            public const string NORN_EPO = "NORN_EPO";
            /// <summary> Nektet, fare for helsepersonell</summary>
            public const string NORN_FFH = "NORN_FFH";
            /// <summary> Nektet, fare for liv</summary>
            public const string NORN_FFL = "NORN_FFL";
            /// <summary> Nektet, foreldet</summary>
            public const string NORN_FOR = "NORN_FOR";
            /// <summary> Nektet, foreldreansvarlig</summary>
            public const string NORN_FORANS = "NORN_FORANS";
            /// <summary> Nektet, forsvarlig pasientbehandling</summary>
            public const string NORN_FPB = "NORN_FPB";
            /// <summary> Nektet, klart utilrådelig</summary>
            public const string NORN_KUT = "NORN_KUT";
            /// <summary> Nektet, ungdom</summary>
            public const string NORN_UNGDOM = "NORN_UNGDOM";
            /// <summary> Sperret</summary>
            public const string NORS = "NORS";
            /// <summary> Utsatt innsyn for innbygger</summary>
            public const string NORU = "NORU";
        }

        public static class Oid
        {
            // The correct "system"-value for OID
            public const string System = "urn:ietf:rfc:3986";

            public const string Fnr = "2.16.578.1.12.4.1.4.1";
            public const string Dnr = "2.16.578.1.12.4.1.4.2";
            public const string Hnr = "2.16.578.1.12.4.1.4.3";
            public const string Hpr = "2.16.578.1.12.4.1.4.4";
            public const string ReshId = "2.16.578.1.12.4.1.4.102";
            public const string Brreg = "2.16.578.1.12.4.1.4.101";
            public const string Nhn = "2.16.578.1.12.4.5";

            public static class Saml
            {
                public static class Acp
                {
                    // Citizen OID values

                    /// <summary>
                    /// CUSTOM OID: No representation overrides (represents themself)
                    /// </summary>
                    public const string NullValue = "urn:oid:2.16.578.1.12.4.1.7.2.1.0";

                    /// <summary>
                    /// Represent citizen under 12 years of age
                    /// </summary>
                    public const string RepresentCitizenUnder12 = "urn:oid:2.16.578.1.12.4.1.7.2.1.1";

                    /// <summary>
                    /// Represent another cititzen (Power of Attorney)
                    /// </summary>
                    public const string RepresentAnotherCitizen = "urn:oid:2.16.578.1.12.4.1.7.2.1.2";

                    /// <summary>
                    /// Represent citizen unable to give consent
                    /// </summary>
                    public const string RepresentedUnableToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.3";

                    // Healthcare practitioner OID values

                    /// <summary>
                    /// Healthcare professional [subject] is not obliged to retrieve patient's consent to [resource] open and see patient's healthcare data, e.g. "patient's regular physician" (fastlege)
                    /// </summary>
                    public const string NotObligedToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.4";

                    /// <summary>
                    /// Healthcare professional [subject] has been given explicit consent from patient [resource] to open and see patient's healthcare data, including locked data
                    /// </summary>
                    public const string ExcplicitConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.5";

                    /// <summary>
                    /// Healthcare professional [subject] is not able to retrieve consent from current patient [resource] (e.g. patient is unconscious)
                    /// </summary>
                    public const string UnableToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.6";

                    /// <summary>
                    /// Healthcare professional [subject] has documented reasons to unlock all available healthcare data for current patient [resource] in an emergency/catastrophic situation
                    /// </summary>
                    public const string ExceptionToConcent = "urn:oid:2.16.578.1.12.4.1.7.2.1.7";

                    /// <summary>
                    /// Healthcare professional [subject] has retrieved consent from patient [resource] to open and see patient's healthcare data
                    /// </summary>
                    public const string HasConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.8";

                }

                public static class Bppc
                {
                    /// <summary>
                    /// CUSTOM OID: Null value
                    /// </summary>
                    public const string NullValue = "urn:oid:2.16.578.1.12.4.1.7.2.2.0";

                    /// <summary>
                    /// Consent from an analog channel
                    /// </summary>
                    public const string AnalogChannel = "urn:oid:2.16.578.1.12.4.1.7.2.2.1";

                    /// <summary>
                    /// Consent from a digital channel
                    /// </summary>
                    public const string DigitalChannel = "urn:oid:2.16.578.1.12.4.1.7.2.2.2";
                }
            }
        }
    }
}
