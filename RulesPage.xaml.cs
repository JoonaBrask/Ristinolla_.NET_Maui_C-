using Windows.Media.PlayTo;

namespace final_work;

public partial class RulesPage : ContentPage
{
    //Constructori s��nn�ille
    public RulesPage()
    {
        InitializeComponent();
    }

    //Tapahtumak�sittelij�, kun Seuraava-painiketta napsautetaan, siirtyy PlayerManager-sivulle
    private async void StartChoosePlayer_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PlayerManager());
    }
}