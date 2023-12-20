using final_work;
using System.Collections.ObjectModel;
using System.Text.Json;


namespace final_work

{
    public partial class MainPage : ContentPage
    {
        //ObservableCollectioni tallentaa pelaajien luettelon
        public ObservableCollection<Player> Players { get; set; }

        //Luokka erilaisille pelaajan ominaisuuksille
        public class Player
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int BirthYear { get; set; }
            public int Wins { get; set; }
            public int Losses { get; set; }
            public int Draws { get; set; }
            public TimeSpan TotalTimePlayed { get; set; }

            //voitot, tappiot, tasapelit ja kokonaispeliaika asetetaan arvoon 0, kun uusi pelaaja luodaan
            public Player()
            {
                Wins = 0;
                Losses = 0;
                Draws = 0;
                TotalTimePlayed = new TimeSpan(0, 0, 0);
            }

            // Yhdistä pelaajan nimi ja syntymävuosi näytettäväksi käyttöliittymässä PlayerManager-sivun valitsimessa
            public string PickerInfo => $"{FirstName} {LastName} {BirthYear}";
        }

        //Constructori MainPagelle
        public MainPage()
        {
            // Alusta komponentit, luo ObservableCollectioni ja lataa pelaajat JSONista
            InitializeComponent();
            Players = new ObservableCollection<Player>();
            LoadPlayersFromJson();
            BindingContext = this;
        }


        // Lataa pelaajatt JSON-tiedostosta, lajittele ne ja lisää ObservableCollectioniin näyttöä varten
        private void LoadPlayersFromJson()
        {
            // Lataa pelaajat JSONista
            List<Player> players = PlayerInfoSerializer.LoadPlayers();

            // Lajittele pelaajalista ensin tietokonesoittimen mukaan, sitten voitot, sukunimi, etunimi, jotta lista on järjestyksessä
            players = players.OrderByDescending(p => p.FirstName == "Computer")
                            .ThenByDescending(p => p.Wins)
                            .ThenBy(p => p.LastName ?? "")
                            .ThenBy(p => p.FirstName ?? "")
                            .ToList();

            // Tyhjennä Players-kokoelma ennen pelaajien lisäämistä
            Players.Clear();

            // Lisää pelaajia Players-kokoelmaan, jos niitä ei vielä ole
            foreach (Player player in players)
            {
                // Tarkista, onko pelaaja jo Players-kokoelmassa
                bool playerExists = Players.Any(p => p.FirstName == player.FirstName &&
                                                    p.LastName == player.LastName &&
                                                    p.BirthYear == player.BirthYear);

                // Lisää pelaaja vain, jos sitä ei vielä ole kokoelmassa
                if (!playerExists)
                {
                    Players.Add(player);
                }
            }
        }


        // Luokka serialisoimaan ja deserialisoimaan pelaajatietoja JSONista ja JSONiin
        public static class PlayerInfoSerializer
        {
            //ristinolla pelaajien json-tiedostopolku
            public static string FolderPath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PelaajaTiedot");
            public static string FilePath { get; } = Path.Combine(FolderPath, "pelloojat.json");

            // Tallenna päivitetyt pelaajat JSON-tiedostoon
            public static void SavePlayers(List<Player> players) //pelaajat järjestyksessä, tarkista yllä
            {
                try
                {
                    // Luo hakemisto pelaajatiedoille, jos sitä ei ole olemassa
                    string directoryPath = Path.GetDirectoryName(FilePath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Lataa olemassa olevat pelaajat JSON-tiedostosta
                    List<Player> existingPlayers = LoadPlayers();

                    // Iteroi päivitetyn pelaajaluettelon läpi
                    foreach (Player player in players)
                    {
                        // Etsi pelaaja, jolla on sama etunimi, sukunimi ja syntymävuosi
                        Player existingPlayer = existingPlayers.FirstOrDefault(p =>
                            p.FirstName == player.FirstName &&
                            p.LastName == player.LastName &&
                            p.BirthYear == player.BirthYear);

                        // Jos soitin on olemassa, päivitä sen tiedot
                        if (existingPlayer != null)
                        {
                            existingPlayer.Wins = player.Wins;
                            existingPlayer.Losses = player.Losses;
                            existingPlayer.Draws = player.Draws;
                            existingPlayer.TotalTimePlayed = player.TotalTimePlayed;
                        }
                        // Jos pelaajaa ei ole olemassa, lisää se olemassa olevaan pelaajaluetteloon
                        else
                        {
                            existingPlayers.Add(player);
                        }
                    }

                    // Tallenna päivitetty pelaajaluettelo tiedostoon
                    string jsonString = JsonSerializer.Serialize(existingPlayers);
                    File.WriteAllText(FilePath, jsonString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in saving players: {ex.Message}");
                    throw;
                }
            }

            // Lataa pelaajat JSON-tiedostosta
            public static List<Player> LoadPlayers()
            {
                // Tarkista, onko tiedosto olemassa, ja jos on, deserialoi sen sisältö
                if (File.Exists(FilePath))
                {
                    // Lue JSON-tiedot tiedostosta ja muunna se Player-objektien luetteloksi
                    string jsonString = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<List<Player>>(jsonString);
                }
                else
                {
                    // Jos tiedostoa ei ole olemassa, palauta tyhjä lista
                    return new List<Player>();
                }
            }
        }

        //Siirry Sääntösivulle
        private async void ToRulesPage_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RulesPage());
        }

        //Poistu sovelluksesta
        private void Exit_Clicked(object sender, EventArgs e)
        {
            Application.Current.Quit();
        }
    }
}