using Newtonsoft.Json;

namespace Middleware.Service.BAP
{
    public class ValidationResponse : BaseResponse
    {
        [JsonProperty("data")]
        public ValidationForm Result { get; set; }
    }

    public class ValidationForm : Form
    {
        public string Command { get; set; }
    }
}
