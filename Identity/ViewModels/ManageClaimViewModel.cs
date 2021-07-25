namespace Identity.ViewModels
{
    public class ManageClaimViewModel
    {
        public ManageClaimViewModel()
        {

        }
        public ManageClaimViewModel(string type, string value, bool @checked)
        {
            this.Type = type;
            this.Value = value;
            this.Checked = @checked;
        }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Checked { get; set; }
    }
}
