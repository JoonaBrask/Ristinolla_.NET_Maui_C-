using final_work;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.Generic;
using System.Threading;
using static final_work.MainPage;

namespace final_work;

public partial class GamePage : ContentPage
{
    //GAEMPAGE muuttujat pelaajatietojen, pelin tilan ja ruudukon tallentamiseen
    private List<MainPage.Player> players = new List<MainPage.Player>();
    private bool isGameStarted = false; // onko peli alkanut
    private DateTime startTime; // pelin aloitusaika
    private bool isFirstPlayerTurn = true;
    private string PelaajaX = "cross.png";
    string PelaajaO = "circle.png";
    string currentPlayer = "cross.png";
    private MainPage.Player player1;
    private MainPage.Player player2;

    private string[,] GridButtons = new string[3, 3]
    {
        { "empty.png", "empty.png", "empty.png" },
        { "empty.png", "empty.png", "empty.png" },
        { "empty.png", "empty.png", "empty.png" }
    };

    //Constructori GamePagelle, alustaa käyttöliittymäkomponentit ja pelaajatiedot
    public GamePage(MainPage.Player player1, MainPage.Player player2)
    {
        InitializeComponent();
        this.player1 = player1; //this.player1 on pelaaja1-objekti GamePagessa
        this.player2 = player2;
        DisableGrid();
        players = PlayerInfoSerializer.LoadPlayers();

        //Jos pelaaja 2 on tietokone, oikealla oleva kuva on tietokone, muuten pelaaja 2
        if (player2.FirstName == "Tekoäly")
        {
            Player2TurnImage.Source = "computer_logo.png";
        }
        else
        {
            Player2TurnImage.Source = "player2_logo.png";
        }

    }

    //Kun Jatka peliin -painiketta napsautetaan, ajastin käynnistyy, ruudukko on käytössä
    public void TimerAndGameBoardStart_Clicked(object sender, EventArgs e)
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            startTime = DateTime.Now; // pelin alkamisaika, jotta aika voidaan laskea
            EnableGrid();


            // Luo uusi ajastin, aseta väliksi 1 sekunti ja käynnistä se
            var dispatcherTimer = Application.Current.Dispatcher.CreateTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += HandleGameTimer;
            dispatcherTimer.Start();
        }
    }

    //Käsittele peliajastinta, päivitä ajastintarra, päivitä pelaajien kokonaispeliaika
    private void HandleGameTimer(object sender, EventArgs args)
        {
            if (isGameStarted) // tarkista onko peli vielä käynnissä
            {
                TimeSpan elapsedTime = DateTime.Now - startTime;
                string formattedTime = $"{(int)elapsedTime.TotalHours:D2}:{elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}";

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TimerLabel.Text = formattedTime;

                    // Päivitä pelaajien kokonaispeliaika
                    player1.TotalTimePlayed = elapsedTime;
                    player2.TotalTimePlayed = elapsedTime;
                });
            }
        }


    //Peli päättyy: tarkista voiton ja tasapelin ehdot, aseta voittajan kuva, poista ruudukko käytöstä pelin päätyttyä
    //Tallenna pelaajatiedot
    //Palauta peli tilalle (tosi tai epätosi)
    private bool isGameOver = false; //game over first setti false
    private bool GameOver()
    {

        // Tarkista, onko peli jo ohi, ja jos on, palauta false
        if (isGameOver)
        {
            return false;
        }


        // Tarkista voitto tai tasapeli
        bool gameOver = CheckForWin() || CheckForTie();

        // Jos peli on ohi, aseta voittajakuva ja poista ruudukko käytöstä
        if (gameOver)
        {
            isGameOver = true; // Aseta lippu estääksesi GameOver()-kutsut.

            //Aseta voittajan kuva nykyisen pelaajan tai tasapelin perusteella
            if (CheckForWin())
            {
                SetWinnerImage(currentPlayer == PelaajaX ? "Player 1" : "Player 2");
            }
            else if (CheckForTie())
            {
                SetWinnerImage("tie");
            }

            WinnerImage.IsVisible = true;

            // Poista ruudukko käytöstä pelin päätyttyä
            DisableGrid();

            // Tallenna pelaajatiedot
            SavePlayerData();

            //isGameStarted = false; // Stop the timer
        }
        return gameOver; // Palauta tosi, jos peli on ohi, false jos ei
    }

    //Tallenna pelaajatiedot pelaajaluetteloon, päivitä olemassa olevat pelaajatiedot tai lisää uusia pelaajia
    //Tallenna päivitetty pelaajaluettelo JSON-tiedostoon ja korvaa olemassa olevan tiedoston
    private void SavePlayerData()
    {
        // Etsi ja päivitä olemassa olevat Player 1 -tiedot
        var existingPlayer1 = players.FirstOrDefault(player =>
            player.FirstName == player1.FirstName &&
            player.LastName == player1.LastName &&
            player.BirthYear == player1.BirthYear);

        // Jos Player 1 on olemassa, päivitä sen tiedot
        if (existingPlayer1 != null)
        {
            existingPlayer1.Wins += (CheckForWin() && currentPlayer == PelaajaX) ? 1 : 0;
            existingPlayer1.Losses += (CheckForWin() && currentPlayer == PelaajaO) ? 1 : 0;
            existingPlayer1.Draws += (CheckForTie()) ? 1 : 0;
            existingPlayer1.TotalTimePlayed += player1.TotalTimePlayed; // Päivitä peliaika
        }
        else
        {
            // Jos pelaajaa 1 ei ole, lisää se luetteloon
            players.Add(player1);
        }

        // Etsi ja päivitä olemassa olevat Player 2 -tiedot
        var existingPlayer2 = players.FirstOrDefault(player =>
            player.FirstName == player2.FirstName &&
            player.LastName == player2.LastName &&
            player.BirthYear == player2.BirthYear);

        // Jos Player 2 on olemassa, päivitä sen tiedot
        if (existingPlayer2 != null)
        {
            existingPlayer2.Wins += (CheckForWin() && currentPlayer == PelaajaO) ? 1 : 0;
            existingPlayer2.Losses += (CheckForWin() && currentPlayer == PelaajaX) ? 1 : 0;
            existingPlayer2.Draws += (CheckForTie()) ? 1 : 0;
            existingPlayer2.TotalTimePlayed += player2.TotalTimePlayed; // Update playtime
        }
        else
        {
            //Jos Player 2:ta ei ole olemassa, lisää ne luetteloon
            players.Add(player2);
        }

        // Tallenna päivitetty pelaajaluettelo JSONiin ja korvaa olemassa oleva tiedosto
        PlayerInfoSerializer.SavePlayers(players);
    }


    //Ota käyttöön ja poista käytöstä ruudukko, käytetään ajastimessa ja pelilaudan aloituspainikkeessa
    private void EnableGrid()
    {
        // Ota käyttöön kaikki ruudukon ImageButtonit
        foreach (var child in ticTacToeGrid.Children)
        {
            if (child is ImageButton button)
            {
                button.IsEnabled = true;
            }
        }
    }
    private void DisableGrid()
    {
        // Poista käytöstä kaikki ruudukon ImageButtonit
        foreach (var child in ticTacToeGrid.Children)
        {
            if (child is ImageButton button)
            {
                button.IsEnabled = false;
            }
        }
    }

    //Erityissiirtomäärän rakenne, käytetään myös RulesPage.xaml.cs:ssä
    public struct SpecialMoveCount
    {
        public int SpecialMove1Count { get; set; }
    }

    private SpecialMoveCount specialMoveCount;


    // Yritä ladata erikoissiirtomäärä tekstitiedostosta
    // Jos tiedostoa ei ole olemassa, aseta erityissiirtomääräksi 0
    private void LoadSpecialMoveCountFromTextFile()
    {
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "specialMoveCounter_Saved.txt");

            if (File.Exists(filePath))
            {
                string textData = File.ReadAllText(filePath);
                if (int.TryParse(textData, out int amountCheatCount))
                {
                    specialMoveCount.SpecialMove1Count = amountCheatCount;
                }
            }
            else
            {
                specialMoveCount.SpecialMove1Count = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading special move count: {ex.Message}");
        }
    }

    // Tallenna erikoissiirtomäärä tekstitiedostoon
    // Jos tiedostoa ei ole olemassa, luo se
    private void SaveSpecialMoveCountToTextFile(int count)
    {
        // Muunna erikoissiirtojen määrä merkkijonoksi tallentamista varten
        string textData = $"SpecialMove1Count: {specialMoveCount.SpecialMove1Count}";

        // Yritä tallentaa erikoissiirtomäärä tekstitiedostoon, jos sitä ei ole, luo se
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "specialMoveCounter_Saved.txt");
            File.WriteAllText(filePath, count.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // Palauta kaikki ruudukon paikat muotoon "empty.png" ennen kuin teet erikoissiirron salaisen koodin syöttämisen jälkeen
    private void ResetGrid()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GridButtons[i, j] = "empty.png";
            }
        }
    }

    // UpdateGridUI-menetelmä päivittää käyttöliittymän GridButtons-taulukon perusteella.
    // Se iteroi gridButtons-taulukon läpi ja asettaa vastaavien ImageButtons-lähteen
    // kuvastaa pelilaudan nykyistä tilaa.
    private void UpdateGridUI()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                ImageButton button = GetGridButtonFromIndices(i, j);
                if (button != null)
                {
                    //Päivitä visuaalinen ruudukko tyhjällä.png-, cross.png- tai circle.png-tiedostolla GridButtons-taulukon sijaintien perusteella
                    button.Source = GridButtons[i, j];
                }
            }
        }
    }

    // Käsittelee tapahtumaa, kun ruudukkopainiketta napsautetaan.
    // Tarkistaa voiton ja tasapelin ehdot. Lisää voittoja, tappioita ja tasatilanteita vastaavasti.
    // Päivittää pelaajan reunat näyttämään nykyisten pelaajien vuoron, vaihtaa nykyisen pelaajan ja heidän merkin.
    // Poistaa ruudukkopainikkeet käytöstä tekoälyn vuorollaan ja ottaa ne käyttöön tekoälyn liikkeen jälkeen.
    private async void GridClicked(object sender, EventArgs e)
    {
        try
        {
            ImageButton GridClicked = (ImageButton)sender; //Hae napsautettu ruudukkopainike
            int row = Grid.GetRow(GridClicked); //Hae napsautetun painikkeen rivi
            int column = Grid.GetColumn(GridClicked); //Hae napsautetun painikkeen sarake

            if (GridButtons[row, column] == "empty.png")
            {
                GridClicked.Source = currentPlayer; // Päivitä ruudukko nykyisen pelaajan merkillä
                GridButtons[row, column] = currentPlayer; // Päivitä napsautetun painikkeen lähde GridButtons-taulukossa

                bool isWin = CheckForWin();
                if (isWin)
                {
                    if (currentPlayer == PelaajaX)
                    {
                        player1.Wins++; //kasvata pelaaja 1 voittoa pelaaja 1 -objektille
                        player2.Losses++; //kasvata pelaajan 2 tappiota pelaaja 2 -objektille
                    }
                    else if (currentPlayer == PelaajaO)
                    {
                        player2.Wins++; //kasvata pelaaja 2 voittoa pelaaja 2 -objektille
                        player1.Losses++; //kasvata pelaajan 1 tappiota pelaaja 1 -objektillet
                    }

                    // jos peli voitetaan, peli on ohi, poista ruudukko käytöstä, aseta voittajan kuva, tallenna pelaajatiedot, pysäytä ajastin
                    isGameStarted = false;

                    GameOver();
                    return;
                }

                // tarkista tasapeli
                bool isTie = CheckForTie();
                if (isTie)
                {
                    player1.Draws++; //kasvata pelaajan 1:n tasapelejä pelaajan 1 objektiin
                    player2.Draws++; //kasvata pelaaja 2:n tasapelejä pelaajan 2 objektiin
                    isGameStarted = false;

                    GameOver();
                    return;
                }

                UpdatePlayerBorder(); // Päivitä pelaajakehykset osoittamaan kenen vuoro on
                // Vaihda nykyinen pelaaja ja hänen merkkinsä, pelaaja 1 on aina X ja pelaaja 2 on aina O, pelaaja 1 aloittaa
                isFirstPlayerTurn = !isFirstPlayerTurn;
                currentPlayer = isFirstPlayerTurn ? PelaajaX : PelaajaO; 

            }

            // Poista ruudukkopainikkeet käytöstä tekoälyn vuorollaan, ota ne käyttöön tekoälyn liikkeen jälkeen
            if (player2.FirstName == "Tekoäly" && currentPlayer == PelaajaO)
            {
                DisableGridButtons();
                await ComputerAIMove();
                EnableGridButtons();
            }
        }
        //Jos tulee poikkeus, näytä virheilmoitus ja palaa pääsivulle
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            await DisplayAlert("ERROR", "The gameboard doesn't seem to be working properly.\nPress OK to continue back to MainPage", "OK");
            await Navigation.PopAsync(); // palaa MainPagelle
        }
    }

    // Poista käytöstä kaikki ruudukon ImageButtonit, joita käytetään tekoälyn kanssa
    private void DisableGridButtons()
    {
        foreach (var child in ticTacToeGrid.Children)
        {
            if (child is ImageButton button)
            {
                button.IsEnabled = false;
            }
        }
    }

    // Ota käyttöön kaikki ruudukon ImageButtonit, joita käytetään tekoälyn kanssa
    private void EnableGridButtons()
    {
        foreach (var child in ticTacToeGrid.Children)
        {
            if (child is ImageButton button)
            {
                button.IsEnabled = true;
            }
        }
    }

    // AI-siirto: hanki tyhjiä paikkoja, viive 0,5-2s, satunnaista sijainti, aseta merkki, päivitä käyttöliittymä visuaaliseen ruudukkoon
    private async Task ComputerAIMove()
    {
        // Hanki tyhjiä paikkoja iteroimalla GridButtons-taulukon läpi
        List<int> emptyPositions = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (GridButtons[i, j] == "empty.png") //paikka on tyhjä, jos kuva on olemassa "empty.png"
                {
                    emptyPositions.Add(i * 3 + j);
                }
            }
        }

        // Jos on tyhjiä paikkoja, siirry viivettä 0,5 s - 2 s, satunnainen sijainti, aseta merkki, päivitä käyttöliittymä visuaaliseen ruudukkoon
        if (emptyPositions.Count > 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(500, 2000))); // viive 0.5s-2s
            Random random = new Random();
            int randomIndex = random.Next(0, emptyPositions.Count); // satunnainen sijainti
            int position = emptyPositions[randomIndex];
            int row = position / 3;
            int column = position % 3;

            // Varmista, että tekoälyn liikkeelle on asetettu oikea merkki
            GridButtons[row, column] = PelaajaO;

            // Päivitä käyttöliittymä GridButtons-taulukon perusteella
            ImageButton button = GetGridButtonFromIndices(row, column);
            if (button != null)
            {
                button.Source = GridButtons[row, column];
            }

            // Tarkista voitto tai tasapeli, jos pelaaja 2 on tietokone
            if (player2.FirstName == "Tekoäly" && CheckForWin())
            //jos pelaaja 2 on tietokone ja tarkista voitto on tosi
            {
                player2.Wins++; // lisää pelaaja 2 voittaa pelaaja 2 -objektin
                player1.Losses++; // lisää pelaaja 1 tappioita pelaaja 1 - objektille
                isGameStarted = false;

                GameOver();
                return;
            }

            CheckForTie();
            UpdatePlayerBorder();
            currentPlayer = (currentPlayer == PelaajaX) ? PelaajaO : PelaajaX;
            isFirstPlayerTurn = !isFirstPlayerTurn;
        }
    }



    //Hae ImageButton ruudukkoindekseistä käytettäväksi tietokoneen tekoälyn siirrossa
    private ImageButton GetGridButtonFromIndices(int row, int column)
    {
        // Hae ImageButton lasketusta sijainnista ruudukossa
        if (ticTacToeGrid.Children[row * 3 + column] is ImageButton button)
        {
            return button;
        }
        else
        {
            return null;
        }
    }

    //Tarkistaa voiton tarkistamalla jommankumman pelaajan rivit, sarakkeet ja diagonaalit
    private bool CheckForWin()
    {
        // Tarkista rivit
        for (int i = 0; i < 3; i++)
        {
            if (GridButtons[i, 0] == PelaajaX && GridButtons[i, 1] == PelaajaX && GridButtons[i, 2] == PelaajaX)
            {
                return true;
            }
            if (GridButtons[i, 0] == PelaajaO && GridButtons[i, 1] == PelaajaO && GridButtons[i, 2] == PelaajaO)
            {
                return true;
            }
        }

        // Tarkista sarakkeet
        for (int i = 0; i < 3; i++)
        {
            if (GridButtons[0, i] == PelaajaX && GridButtons[1, i] == PelaajaX && GridButtons[2, i] == PelaajaX)
            {
                return true;
            }
            if (GridButtons[0, i] == PelaajaO && GridButtons[1, i] == PelaajaO && GridButtons[2, i] == PelaajaO)
            {
                return true;
            }
        }

        // Tarkista diagonaalit
        if (GridButtons[0, 0] == PelaajaX && GridButtons[1, 1] == PelaajaX && GridButtons[2, 2] == PelaajaX)
        {
            return true;
        }
        if (GridButtons[0, 0] == PelaajaO && GridButtons[1, 1] == PelaajaO && GridButtons[2, 2] == PelaajaO)
        {
            return true;
        }
        if (GridButtons[0, 2] == PelaajaX && GridButtons[1, 1] == PelaajaX && GridButtons[2, 0] == PelaajaX)
        {
            return true;
        }
        if (GridButtons[0, 2] == PelaajaO && GridButtons[1, 1] == PelaajaO && GridButtons[2, 0] == PelaajaO)
        {
            return true;
        }

        return false;
    }


    //Tarkista tasapeli tarkistamalla, onko jäljellä tyhjiä paikkoja, jos ei, se on tasapeli
    private bool CheckForTie()
    {
        //Jos ei voitto, tarkista tasapeli
        if (!CheckForWin())
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (GridButtons[i, j] == "empty.png")
                    {
                        return false;
                    }
                }
            }
            // Jos tyhjiä paikkoja ei löydy, se on tasapeli
            return true;
        }
        // Jos on voitto, se ei ole tasapeli
        return false;
    }


    //Aseta voittajan tai tasapelin kuva voittaneiden/tasapelien pelaajien perusteella, käytetty GameOver()
    private void SetWinnerImage(string winner)
    {
        if (winner == "Player 1")
        {
            WinnerImage.Source = "player_one_victory.png";
        }
        else if (winner == "Player 2")
        {
            WinnerImage.Source = "player_two_victory.png";
        }
        else if (winner == "tie")
        {
            WinnerImage.Source = "tie_notification.png";
        }
    }

    //Päivittää pelaajan kuvan reunukset osoittamaan kenen vuoro on, korosta nykyinen pelaaja, tyhjentää toisen pelaajan
    private void UpdatePlayerBorder()
    {
        Player1Frame.Stroke = null;
        Player2Frame.Stroke = null;

        if (isFirstPlayerTurn)
        {
            Player2Frame.Stroke = Color.FromArgb("#0026be");
            Player1Frame.Stroke = Color.FromArgb("#00000000");
        }
        else if (!isFirstPlayerTurn)
        {
            Player1Frame.Stroke = Color.FromArgb("#0026be");
            Player2Frame.Stroke = Color.FromArgb("#00000000");
        }
    }

    //Kun pääsivu ladataan uudelleen, myös tuloslistanäkymä päivitetään
    private void BackToMainpage_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MainPage());
    }

}