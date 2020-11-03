﻿using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ChapubelichBot.Init
{
    [Serializable]
    class ExceptionLogger
    {
        public string Time { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string CallStack { get; set; }
        public ExceptionLogger InnerExeption { get; set; }

        public ExceptionLogger(Exception ex)
        {
            Time = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            Source = ex.Source;
            Message = ex.Message;
            CallStack = ex.StackTrace;
            if (ex.InnerException != null)
                InnerExeption = new ExceptionLogger(ex.InnerException);
        }

        public void WriteData(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            string fileName = Path.GetFileNameWithoutExtension(path) + DateTime.Now.ToString("MM.dd.yyyy HH.mm.ss") + ".json";
            path = Path.Combine(directoryName, fileName);
            string errorJson = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
            Console.WriteLine(errorJson);
            File.AppendAllText(path, errorJson);
        }
    }
}
