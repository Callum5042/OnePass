using System.Text.Json.Serialization;

namespace OnePass.Services
{
    public class OnePassSettings
    {
        [JsonIgnore]
        public bool IsLoggedIn { get; set; } = false;

        [JsonIgnore]
        public string MasterPassword { get; set; }// = "password123";

        public string FileName { get; set; } = "data.bin";

        public string TestLoad { get; set; }

        [JsonIgnore]
        public string Salt { get; set; } = @"0wQuU:8,]>Rz2,f@?tDL6Di9?4QhCtg',jFcPP;@TAM}8Z'eZ9nGIpW4FVU@Xls]8B;p3m.q>12pg?4;q.5@1vj]g7vRQ0ubEum.cA]uo[<pFAe#;.mCOvL;4YOeLT?fW.@Ni5v,HcF{r.s8Ql[[WZfS<HmTmy:kw0w;}yJaPCel3zacmo<7gN]d@t}Rtwiii.nQnq@TUb9TMFxFq#NYv9V?0e~dy~<YJOr5'@x4x9drYV5VXOw#A4c5,Rpb;Rdr";

        public string Hash { get; set; }
    }
}
