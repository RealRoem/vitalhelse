namespace VitalHelse.Models;

public class UserAddressViewModel
{
    public List<UserAddress> Existing { get; set; } = new();
    public UserAddress NewAddress { get; set; } = new(); 
    public int? SelectedAddressId { get; set; }
}