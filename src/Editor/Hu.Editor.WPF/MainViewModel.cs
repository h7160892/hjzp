using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Hu.Editor.Models;

namespace Hu.Editor.WPF;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<PersonViewModel> Persons { get; set; } = new();
    public PersonViewModel? SelectedPerson { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        // 模拟数据
        Persons.Add(new PersonViewModel 
        { 
            Id = 1, Name = "胡公一世", GenderStr = "男", BirthDate = "1650-01-15", 
            DeathDate = "1720-03-20", Generation = "1", Residence = "泗县草沟镇于韩村",
            StatusStr = "已故"
        });
        Persons.Add(new PersonViewModel 
        { 
            Id = 2, Name = "胡公二世", GenderStr = "男", BirthDate = "1680-06-10", 
            DeathDate = "1755-11-25", Generation = "2", Residence = "泗县草沟镇于韩村",
            StatusStr = "已故"
        });
        Persons.Add(new PersonViewModel 
        { 
            Id = 3, Name = "胡公三世", GenderStr = "男", BirthDate = "1710-03-22", 
            DeathDate = "", Generation = "3", Residence = "泗县草沟镇于韩村",
            StatusStr = "已故"
        });
        Persons.Add(new PersonViewModel 
        { 
            Id = 4, Name = "胡建国", GenderStr = "男", BirthDate = "1950-08-12", 
            DeathDate = "", Generation = "10", Residence = "宿州市埇桥区",
            StatusStr = "在世"
        });
        Persons.Add(new PersonViewModel 
        { 
            Id = 5, Name = "胡建军", GenderStr = "男", BirthDate = "1955-02-28", 
            DeathDate = "", Generation = "10", Residence = "泗县草沟镇于韩村",
            StatusStr = "在世"
        });
    }
}

public class PersonViewModel : INotifyPropertyChanged
{
    private string _name = "";
    private string _birthDate = "";
    private string _deathDate = "";
    private string _generation = "";
    private string _residence = "";
    private string _statusStr = "";
    private string _genderStr = "";

    public int Id { get; set; }
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
    public string BirthDate { get => _birthDate; set { _birthDate = value; OnPropertyChanged(); } }
    public string DeathDate { get => _deathDate; set { _deathDate = value; OnPropertyChanged(); } }
    public string Generation { get => _generation; set { _generation = value; OnPropertyChanged(); } }
    public string Residence { get => _residence; set { _residence = value; OnPropertyChanged(); } }
    public string StatusStr { get => _statusStr; set { _statusStr = value; OnPropertyChanged(); } }
    public string GenderStr { get => _genderStr; set { _genderStr = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
