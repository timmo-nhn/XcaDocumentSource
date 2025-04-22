//using NHapi.Base.Model;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using XcaXds.Commons.Models.Hl7;
//using XcaXds.Commons.Models.Hl7.Message;

//namespace XcaXds.Commons.Commons;

//public class Hl7PipeParser
//{
//    public char Field { get; set; }
//    public char Component { get; set; }
//    public char Subcomponent { get; set; }
//    public char Repetitions { get; set; }
//    public char Escape { get; set; }
//    public char Segment { get; set; }

//    public Hl7Message Parse(string message)
//    {
//        var msg = new Hl7Message();
//        var segments = message.Split(Segment, StringSplitOptions.RemoveEmptyEntries);

//        foreach (var seg in segments)
//        {
//            var segment = new Hl7Segment();
//            var fields = seg.Split(Field);

//            segment.Name = fields[0];

//            for (int i = 1; i < fields.Length; i++)
//            {
//                var field = new Hl7Field();
//                var repetitions = fields[i].Split(REPETITION);

//                foreach (var rep in repetitions)
//                {
//                    var component = new Hl7Component();
//                    component.Components.AddRange(rep.Split(COMPONENT));
//                    field.Repetitions.Add(component);
//                }

//                segment.Fields.Add(field);
//            }

//            msg.Segments.Add(segment);
//        }

//        return msg;
//    }
//}

