namespace NewsWire.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> AllRoles { get; set; }
        public List<string> UserRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}
