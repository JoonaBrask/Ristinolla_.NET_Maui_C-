
using final_work;
using Windows.Gaming.Input;

namespace final_work
{
    // PlayerManager hoitaa pelaajien valinnan ja uusien pelaajien luomisen
    public partial class PlayerManager : ContentPage
    {
        private List<MainPage.Player> PickerInfo { get; set; } // Luettelo pelaajatietojen tallentamiseen poimijaa varten
        private MainPage.Player player1;
        private MainPage.Player player2;
        private bool isComputerSelected = false; //Boolean tarkistaa, onko tietokonepelaaja valittu

        //Constructori playermanagerille
        public PlayerManager()
        {
            InitializeComponent();

            PickerInfo = new List<MainPage.Player>(); //Alustus PickerInfo-luettelolle
            LoadPlayersFromJson(); //Lataa pelaajan tiedot JSON-tiedostosta

            // Aseta item pickerin tuotelähde PickerInfo-luetteloon
            playerOnePicker.ItemsSource = PickerInfo;
            playerTwoPicker.ItemsSource = PickerInfo;

            // Käsittelee tapahtumaa, kun pelaaja valitaan pickeristä
            playerOnePicker.SelectedIndexChanged += (sender, args) =>
            {
                //Jos pelaaja valitaan, aseta tekstikentät pelaajan tietoihin
                if (playerOnePicker.SelectedItem is MainPage.Player selectedPlayer)
                {
                    //Päivitä tekstikentät valitun pelaajan tiedoilla
                    FirstNameEntry_P1.Text = selectedPlayer.FirstName;
                    LastNameEntry_P1.Text = selectedPlayer.LastName;
                    BirthYearEntry_P1.Text = selectedPlayer.BirthYear.ToString();
                }
            };

            playerTwoPicker.SelectedIndexChanged += (sender, args) =>
            {
                if (playerTwoPicker.SelectedItem is MainPage.Player selectedPlayer)
                {
                    FirstNameEntry_P2.Text = selectedPlayer.FirstName;
                    LastNameEntry_P2.Text = selectedPlayer.LastName;
                    BirthYearEntry_P2.Text = selectedPlayer.BirthYear.ToString();
                }
            };

            BindingContext = this; //Aseta sidoskonteksti tälle sivulle tämän sivun XAML-elementeille
        }

        //Lataa pelaajat JSON-tiedostosta ja lisää ne PickerInfo-luetteloon
        private void LoadPlayersFromJson()
        {
            List<MainPage.Player> players = MainPage.PlayerInfoSerializer.LoadPlayers();
            PickerInfo.AddRange(players);
        }

        //Hndleri, kun kuvapainiketta ComputerPlayerSelection_Button napsautetaan.
        //Vaihtaa painikkeen taustaväriä ja poistaa tekstikentät käytöstä pelaajalta 2
        //Jos painiketta napsautetaan uudelleen, taustaväri palautetaan nollaksi ja tekstikentät otetaan käyttöön
        //Asettaa player2:n tietokonepelaajaksi
        private void ComputerPlayerSelected_Clicked(object sender, EventArgs e)
        {
            isComputerSelected = !isComputerSelected;

            if (isComputerSelected)
            {
                //Väritä painike, kun tietokonepelaaja on valittuna, ja poista pelaajan 2 tekstikentät käytöstä
                ComputerPlayerSelection_Button.BackgroundColor = Color.FromArgb("#0026be");
                playerTwoPicker.IsEnabled = false;
                playerTwoPicker.SelectedItem = null;
                FirstNameEntry_P2.IsEnabled = false;
                FirstNameEntry_P2.Text = "";
                LastNameEntry_P2.IsEnabled = false;
                LastNameEntry_P2.Text = "";
                BirthYearEntry_P2.IsEnabled = false;
                BirthYearEntry_P2.Text = "";

                //Tarkista, onko tietokonepelaaja jo luettelossa
                MainPage.Player computerPlayer = PickerInfo.FirstOrDefault(player => player.FirstName == "Tekoäly");

                //Jos tietokonepelaajaa ei ole luettelossa, luo uusi tietokonepelaaaja ja lisää se luetteloon
                if (computerPlayer == null)
                {
                    computerPlayer = new MainPage.Player
                    {
                        FirstName = "Tekoäly",
                        LastName = "",
                        BirthYear = 9999 
                    };
                    PickerInfo.Insert(0, computerPlayer); // Lisää tietokonepelaaja luettelon alkuun
                }

                player2 = computerPlayer; // Päivitä player2-muuttuja tietokonepelaajalla
            }
            //Jos painiketta napsautetaan uudelleen, aseta taustaväri takaisin nollaksi ja ota tekstikentät käyttöön pelaajalle 2
            else
            {
                ComputerPlayerSelection_Button.BackgroundColor = null;
                playerTwoPicker.IsEnabled = true;
                FirstNameEntry_P2.IsEnabled = true;
                LastNameEntry_P2.IsEnabled = true;
                BirthYearEntry_P2.IsEnabled = true;
            }
        }

        // Käsittelijä, kun Jatka-painiketta napsautetaan
        //Tarkistaa, ovatko pelaajatiedot oikein, luo pelaajaobjektit ja navigoi GamePagelle
        //Jos pelaajatiedot eivät ole kelvollisia, näytä hälytys
        //Tarkistaa virheellisiä syötteitä, kuten tyhjiä tekstikenttiä, virheellisiä merkkejä ja virheellistä syntymävuotta
        //Tarkistaa myös, ovatko pelaajan 1 ja pelaajan 2 tiedot samat ja onko pelaajan 1 tai pelaajan 2 etunimi "tietokone"
        private async void GamePageButton_Clicked(object sender, EventArgs e)
        {
            //Hanki pelaaja 1:n tiedot syöttökentistä
            string player1FirstName = FirstNameEntry_P1.Text;
            string player1LastName = LastNameEntry_P1.Text;
            string player1BirthYearText = BirthYearEntry_P1.Text;

            //Tarkista, ovatko pelaajan 1 tiedot oikein
            if (!IsValidName(player1FirstName) || !IsValidName(player1LastName) || !IsValidBirthYear(player1BirthYearText))
            {
                await DisplayAlert("Virheellinen syöte", "Pelaaja 1: Anna nimelle kelvoliset merkkijonot ja 4-numeroinen syntymävuosi.", "OK");
                return;
            }

            // Tarkista, ovatko pelaaja 1 ja pelaaja 2 samat
            if (FirstNameEntry_P1.Text.Equals(FirstNameEntry_P2.Text, StringComparison.OrdinalIgnoreCase) &&
                LastNameEntry_P1.Text.Equals(LastNameEntry_P2.Text, StringComparison.OrdinalIgnoreCase) &&
                BirthYearEntry_P1.Text.Equals(BirthYearEntry_P2.Text, StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Virheellinen syöte", "Pelaajilla 1 ja 2 ei voi olla samat tiedot", "OK");
                return;
            }

            // Tarkista, yrittääkö jompikumpi pelaaja kirjoittaa etunimekseen "Tekoäly".
            if (FirstNameEntry_P1.Text.Equals("Tekoäly", StringComparison.OrdinalIgnoreCase) ||
                FirstNameEntry_P2.Text.Equals("Tekoäly", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Virheellinen syöte", "Pelaajalla 1 ja 2 ei voi olla nimi 'Tekoäly'.", "OK");
                return;
            }

            int player1BirthYear = int.Parse(player1BirthYearText);

            //Luo uusi pelaaja 1 -objekti tiedoilla
            player1 = new MainPage.Player
            {
                FirstName = player1FirstName,
                LastName = player1LastName,
                BirthYear = player1BirthYear
            };

            //Jos tietokonepelaaja on valittuna, aseta pelaaja 2 tietokonepelaajaksi
            if (isComputerSelected)
            {
                player2 = PickerInfo.FirstOrDefault(player => player.FirstName == "Tekoäly");
            }

            //Jos tietokonepelaajaa ei ole valittu, hanki pelaaja 2:n tiedot syöttökentistä
            //Tarkista, että pelaaja 2:n tiedot ovat oikein ja luo pelaaja 2 -objekti
            else
            {
                string player2FirstName = FirstNameEntry_P2.Text;
                string player2LastName = LastNameEntry_P2.Text;
                string player2BirthYearText = BirthYearEntry_P2.Text;

                if (string.IsNullOrWhiteSpace(player2FirstName) || string.IsNullOrWhiteSpace(player2LastName) || string.IsNullOrWhiteSpace(player2BirthYearText))
                {
                    await DisplayAlert("Virhellinen syöte", "Pelaaja 2 täytyy olla valittuna tai erikseen luotu.", "OK");
                    return;
                }

                if (!IsValidName(player2FirstName) || !IsValidName(player2LastName) || !IsValidBirthYear(player2BirthYearText))
                {
                    await DisplayAlert("Virheellinen syöte", "Pelaaja 2: Anna nimelle kelvoliset merkkijonot ja 4-numeroinen syntymävuosi.", "OK");
                    return;
                }

                int player2BirthYear = int.Parse(player2BirthYearText);
                player2 = new MainPage.Player
                {
                    FirstName = player2FirstName,
                    LastName = player2LastName,
                    BirthYear = player2BirthYear
                };
            }

            //Näytä valitut pelaajat ilmoituksena ja siirry GamePagelle
            await DisplayAlert("Pelaajat valittu!", $"Pelaajan 1 nimi: {player1.FirstName} {player1.LastName}\nPelaajan 2 nimi: {player2.FirstName} {player2.LastName}", "OK");

            //Siirry GamePagelle pelaajaobjektien kanssa
            await Navigation.PushAsync(new GamePage(player1, player2));
        }

        //Tarkistaa, ettei nimi ole tyhjä, ei sisällä välilyöntejä ja sisältääkö vain kirjaimia
        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && !name.Contains(" ") && name.All(char.IsLetter);
        }


        //Tarkistaa, onko syntymävuosi 4-numeroinen luku
        private bool IsValidBirthYear(string birthYear)
        {
            int year;
            return int.TryParse(birthYear, out year) && birthYear.Length == 4;
        }
    }
}