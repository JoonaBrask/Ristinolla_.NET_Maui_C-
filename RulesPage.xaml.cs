using Windows.Media.PlayTo;

namespace final_work;

public partial class RulesPage : ContentPage
{
    //Constructori säännöille
    public RulesPage()
    {
        InitializeComponent();
    }

    //Tapahtumakäsittelijä, kun Seuraava-painiketta napsautetaan, siirtyy PlayerManager-sivulle
    private async void StartChoosePlayer_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PlayerManager());
    }
}