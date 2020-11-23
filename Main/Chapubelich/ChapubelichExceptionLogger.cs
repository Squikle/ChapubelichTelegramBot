using System;
using System.IO;
using System.Text.Json;

namespace ChapubelichBot.Main.Chapubelich
{
    [Serializable]
    class ChapubelichExceptionLogger
    {
        public string Time { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string CallStack { get; set; }
        public ChapubelichExceptionLogger InnerExeption { get; set; }

        public ChapubelichExceptionLogger(Exception ex)
        {   
            Time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            Type = ex.GetType().ToString();
            Message = ex.Message;
            Source = ex.Source;
            CallStack = ex.StackTrace;
            if (ex.InnerException != null)
                InnerExeption = new ChapubelichExceptionLogger(ex.InnerException);
        }

        public void WriteData(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            string fileName = Path.GetFileNameWithoutExtension(path) + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss") + ".json";
            path = Path.Combine(directoryName ?? string.Empty, fileName);
            string errorJson = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
            Console.WriteLine(errorJson);
            File.AppendAllText(path, errorJson);
        }
    }
}
