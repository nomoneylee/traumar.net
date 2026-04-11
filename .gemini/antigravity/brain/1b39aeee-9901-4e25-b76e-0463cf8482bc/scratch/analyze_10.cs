using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

class Program {
    static void Main() {
        var json = JObject.Parse(File.ReadAllText(@"c:\Users\nomon\source\repos\traumar.net\tests\golden_files\indicator_10.json"));
        var input = (JArray)json["input"];
        
        int rFull = 0;
        int dotNetFull = 0;
        
        foreach (var item in input) {
            string level = (string)item["level"];
            string transfer = (string)item["transfer_out"];
            
            // Shared filter
            if (!(new[] { "I", "II", "III", "IV" }.Contains(level) && transfer == "No")) continue;
            
            string activation = (string)item["activation_level"];
            
            // R logic (approximated)
            bool rIsFull = activation != null && activation.ToLower().Contains("level 1");
            
            // .NET logic
            bool dotNetIsFull = activation != null && activation.IndexOf("level 1", StringComparison.OrdinalIgnoreCase) >= 0;
            
            if (rIsFull) rFull++;
            if (dotNetIsFull) dotNetFull++;
            
            if (rIsFull != dotNetIsFull) {
                Console.WriteLine($"Mismatch: '{activation}' - R:{rIsFull}, .NET:{dotNetIsFull}");
            }
        }
        
        Console.WriteLine($"R Full: {rFull}, .NET Full: {dotNetFull}");
    }
}
