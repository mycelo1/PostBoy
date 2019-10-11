using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace PostBoy.Helpers
{
    public static class Beautifier
    {
        public static string Xml(string input)
        {
            return StringToSteam(input, DoXml);
        }

        public static string Json(string input)
        {
            return StringToSteam(input, DoJson);
        }

        private static string StringToSteam(string input, Action<Stream, Stream> action)
        {
            using var memory_stream_input = new MemoryStream();
            using var memory_stream_output = new MemoryStream();
            using var stream_writer = new StreamWriter(memory_stream_input);
            using var stream_reader = new StreamReader(memory_stream_output);
            stream_writer.Write(input);
            stream_writer.Flush();
            memory_stream_input.Position = 0;
            action(memory_stream_input, memory_stream_output);
            memory_stream_output.Flush();
            memory_stream_output.Position = 0;
            return stream_reader.ReadToEnd();
        }

        private static void DoXml(Stream input, Stream output)
        {
            var xml_text_writer = new XmlTextWriter(output, Encoding.UTF8);
            xml_text_writer.Formatting = Formatting.Indented;
            var xml_document = new XmlDocument();
            xml_document.Load(input);
            xml_document.WriteContentTo(xml_text_writer);
            xml_text_writer.Flush();
        }

        private static void DoJson(Stream input, Stream output)
        {
            using var utf8_json_writer = new Utf8JsonWriter(output, new JsonWriterOptions() { Indented = true });
            using var json_document = JsonDocument.Parse(input);
            json_document.WriteTo(utf8_json_writer);
        }

    }
}
