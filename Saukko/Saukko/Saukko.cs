﻿using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System.Threading;

// @author Sneck Annika
// @version 25.11.2019

/// <summary>
// Saukko-peli
// Pelissä ohjataan saukkoa, jonka täytyy uida vedessä syömässä kalaa ja näin kerätä pisteitä. Saukon on varottava lähestymästä pallokalaa, joka on myrkyllinen sekä on uitava pakoon
//haita. Jos syö pallokalan tai hai saa kiinni, peli päättyy. Saukon on myös käytävä aika ajoin (45 s välein) maalla haukkaamassa happea ja tätä seurataan ajastimesta. 
//Jos saukko on vedessä ajastimen mennessä nollaan, peli päättyy. Pelin päättyessä ruutuun tulee teksti "Hävisit pelin!" ja voi tallentaa saadun pistemäärän. 
//Pelissä on maksimipistemäärä, jota tavoitellaan. Jos sen saavuttaa, ruutuun tulee teksti "Onnea, olet järven paras kalastajasaukko! Tallenna pisteesi painamalla välilyöntinäppäintä."
 
// TODO: taulukko, ks: https://tim.jyu.fi/answers/kurssit/tie/ohj1/2019s/demot/demo7?answerNumber=8&task=matriisiensumma&user=anmasnec
/// </summary>


public class Saukko : PhysicsGame
{
    // <summary>
    /// Aloitetaan peli. Kutsutaan kentälle saukko, kalat sekä viholliset. Saukon aloituspiste on maalla, josta lähdetään nuolinäppäimillä veteen jahtaamaan kaloja.
    /// </summary>

    // Kuvien lähde: Pixabay.com is an international, copyleft and free-to-use website for sharing photos, illustrations, vector graphics, and film footage.  
    private static readonly Image taustaKuva = LoadImage("JarviSuur");  // https://pixabay.com/fi/photos/ranska-haute-savoie-aravis-lake-4566669/ 30.10.2019
    private static readonly Image herkkuKalaKuva = LoadImage("herkkukala"); //https://pixabay.com/fi/photos/yksitt%C3%A4inen-akvaarioissa-kalat-2377242/ 30.10.2019
    private static readonly Image perusKalaKuva = LoadImage("kala"); // https://pixabay.com/fi/photos/thunnus-tonnikala-kalat-69319/ 30.10.2019
    private static readonly Image palloKalaKuva = LoadImage("PallokalaMuok"); // https://pixabay.com/fi/photos/pufferfish-pallokala-kala-74950/ 30.10.2019
    private static readonly Image haiKuva = LoadImage("HaiMuok"); // https://pixabay.com/fi/photos/silkkinen-hain-shark-utelias-meren-541863/ 30.10.2019
    private static readonly Image tukkiKuva = LoadImage("PidempiTukkiMuokattu"); // https://pixabay.com/fi/photos/lokit-kelan-vipu-pitkin-hakkuu-957496/ 30.10.2019 
    private static readonly Image saukkoKuva = LoadImage("SaukkoMuok");// https://pixabay.com/fi/photos/el%C3%A4in-saukko-nis%C3%A4k%C3%A4s-el%C3%A4intarha-755677/ 30.10.2019

    EasyHighScore topLista = new EasyHighScore();

    const int kalat = 25;
    const int pallokalat = 6;
    const int herkkukalat = 8;
    const int tukit = 6;

    string syotavaKalaTag = "SyotavaKala";
    string herkkuKalaTag = "SyotavaHerkkukala";
    string myrkkyKalaTag = "MyrkkyKala";
    string haiTag = "Hai";
    string tukkiTag = "Tukki";
    string saukkoTag = "Saukko";
    public override void Begin()
    {
        Valikko();
    }


    void LuoKentta()
    {
        Level.Background.Image = taustaKuva;
        Level.CreateBorders();

        BoundingRectangle alaosa = new BoundingRectangle(new Vector(Level.Left, 0), Level.BoundingRect.BottomRight);
        BoundingRectangle ylaosa = new BoundingRectangle(Level.BoundingRect.TopLeft, new Vector(Level.Right, 0));


        PhysicsObject saukko = LuoSaukko(this, saukkoTag);
        saukko.Image = saukkoKuva;


        AddCollisionHandler(saukko, syotavaKalaTag, SaukonKalastus);
        AddCollisionHandler(saukko, herkkuKalaTag, SaukonKalastus);
        AddCollisionHandler(saukko, myrkkyKalaTag, SaukonKalastus);
        AddCollisionHandler(saukko, haiTag, SaukonKalastus);


        PhysicsObject LapinakyvaPalkki = LuoLapinakyvaPalkki(this);

        for (int i = 0; i < kalat; i++)
        {
            PhysicsObject kala = Kala(syotavaKalaTag, saukko, RandomGen.NextDouble(10, 60), RandomGen.NextDouble(10, 30), perusKalaKuva);
        }


        for (int i = 0; i < herkkukalat; i++)
        {
            PhysicsObject kala = Kala(herkkuKalaTag, saukko, RandomGen.NextDouble(50, 60), RandomGen.NextDouble(20, 30), herkkuKalaKuva);
        }


        for (int i = 0; i < pallokalat; i++)
        {
            PhysicsObject kala = Kala(myrkkyKalaTag, saukko, 50, 40, palloKalaKuva);
        }

        
        PhysicsObject hai = Kala(haiTag, saukko, 140, 80, haiKuva);
  
        
        for (int i = 0; i < tukit; i++)
        {
            PhysicsObject tukki = LuoTukki(this, alaosa, 100, tukkiTag); 
            tukki.Image = tukkiKuva;
        }


        LuoPistelaskuri();
        LuoHappiaikaLaskuri(saukko);
        LuoLaskuriKaloille(alaosa, saukko, syotavaKalaTag, herkkuKalaTag);
        LuoLaskuriPallokaloille(alaosa, saukko, myrkkyKalaTag);
       

        Keyboard.Listen(Key.Left, ButtonState.Pressed, LiikutaSaukkoa, "Liikuta saukkoa vasemmalle", saukko, new Vector(-150, 0));
        Keyboard.Listen(Key.Right, ButtonState.Pressed, LiikutaSaukkoa, "Liikuta saukkoa oikealle", saukko, new Vector(150, 0));
        Keyboard.Listen(Key.Up, ButtonState.Pressed, LiikutaSaukkoa, "Liikuta saukkoa ylös", saukko, new Vector(0, 150));
        Keyboard.Listen(Key.Down, ButtonState.Pressed, LiikutaSaukkoa, "Liikuta saukkoa alas", saukko, new Vector(0, -150));

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Lopeta peli");
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, AloitaPeli, "Aloita uusi peli");
        Keyboard.Listen(Key.Space, ButtonState.Pressed, ParhaatPisteet, "Tallenna pisteet");

    }


    /// <summary>
    /// Aliohjelma luo pelaajan eli saukon.
    /// </summary>

    private static PhysicsObject LuoSaukko(PhysicsGame peli, string tag)
    {
        PhysicsObject saukko = new PhysicsObject(80, 40, Shape.Rectangle);
        saukko.Position = new Vector(-200, 320);
        //saukko.Angle = RandomGen.NextAngle();
        saukko.Color = Color.Brown;
        //Vector suunta = RandomGen.NextVector(0, vauhti);
        saukko.Mass = 10.0;
        saukko.Velocity = new Vector(0, 0);
        saukko.CanRotate = false;
        saukko.CollisionIgnoreGroup = 1;
        saukko.Tag = tag;
        saukko.Image = saukkoKuva; // https://pixabay.com/fi/photos/el%C3%A4in-saukko-nis%C3%A4k%C3%A4s-el%C3%A4intarha-755677/ 30.10.2019
        peli.Add(saukko);
        return saukko;
    }


    /// <summary>
    /// Aliohjelma liikuttaa saukkoa.
    /// </summary>
    /// <param name="saukko">Liikutettava pelaaja</param>
    /// <param name="suunta">saukon suunta</param>
    private static void LiikutaSaukkoa(PhysicsObject saukko, Vector suunta)
    {
        //Vector suunta = (10, 10);
        saukko.Move(suunta);
        //saukko.Velocity = new Vector();
        saukko.LinearDamping = 0.99; //0.95
        //bool nytVasen = suunta.X < 0;
        //if (nytVasen ^ vasen) this.MirrorImage();
        //vasen = nytVasen;
    }


    /// <summary>
    /// Aliohjelma luo ylös maan ja veden väliin läpinäkyvän palkin. Vain saukko voi läpäistä sen, eli saukko pääsee maalle mutta muut ei.
    /// </summary>
    private static PhysicsObject LuoLapinakyvaPalkki(PhysicsGame peli)
    {
        double leveys = 1000;
        double korkeus = 30;
        PhysicsObject LapinakyvaPalkki = new PhysicsObject(leveys, korkeus, Shape.Rectangle);
        LapinakyvaPalkki.Position = new Vector(0, 280);
        LapinakyvaPalkki.Color = Color.Transparent;
        LapinakyvaPalkki.CanRotate = false;
        LapinakyvaPalkki.Mass = double.PositiveInfinity;
        LapinakyvaPalkki.CollisionIgnoreGroup = 1;
        peli.Add(LapinakyvaPalkki);
        return LapinakyvaPalkki;
    }


    /// <summary>
    /// Aliohjelma luo kalan. 
    /// </summary>
    /// 
    private PhysicsObject LuoKala (PhysicsGame peli, BoundingRectangle rect, Vector vauhti, string tag, double leveys, double korkeus, Color vari, Image kuva)
    {
        //double leveys = RandomGen.NextDouble(10, 60);
        //double high = korkeus; RandomGen.NextDouble(10, 30);
        PhysicsObject Kala = new PhysicsObject(leveys, korkeus, Shape.Circle);
        Kala.Position = RandomGen.NextVector(rect);
        Kala.Color = vari;
        Kala.Velocity = vauhti; RandomGen.NextVector(20, 0);
        Kala.CanRotate = false;
        Kala.Tag = tag;
        Kala.Image = kuva;
        peli.Add(Kala);
        return Kala;
    }
  

    /// <summary>
    /// Kutsutaan LuoKala-funktiota ja parametreilla määritetään tietty kala.
    /// </summary>
    /// 
    private PhysicsObject Kala(string tag, PhysicsObject saukko, double leveys, double korkeus, Image Kuva)
    {
        BoundingRectangle alaosa = new BoundingRectangle(new Vector(Level.Left, 0), Level.BoundingRect.BottomRight);
        PhysicsObject kala = LuoKala(this, alaosa, RandomGen.NextVector(20, 0), tag, leveys, korkeus, Color.Gray, Kuva);

        if (tag == haiTag)
        {
            FollowerBrain seuraajanAivot = new FollowerBrain(saukko);
            kala.Brain = seuraajanAivot;
            seuraajanAivot.DistanceFar = 200;
            
        }
        return kala;
    }
   

    /// <summary>
    /// Aliohjelma luo vedessä kelluvia tukkeja jotka hidastavat saukon sekä muidenkin liikkumista, koska niitä joutuu kiertää. 
    /// </summary>
    private static PhysicsObject LuoTukki(PhysicsGame peli, BoundingRectangle rect, double vauhti, string tag)
    {
        double leveys = RandomGen.NextDouble(60, 160);
        double korkeus = RandomGen.NextDouble(30, 60);
        PhysicsObject tukki = new PhysicsObject(leveys, korkeus, Shape.Rectangle);
        tukki.Position = RandomGen.NextVector(rect);
        //hai.Angle = RandomGen.NextAngle();
        tukki.Color = Color.DarkBrown;
        //Vector suunta = RandomGen.NextVector(0, vauhti);
        tukki.Velocity = RandomGen.NextVector(10, 0);
        tukki.CanRotate = false;
        tukki.Mass = 60.0;
        //hai.Hit(suunta);
        tukki.Tag = tag;
        tukki.Image = tukkiKuva; // https://pixabay.com/fi/photos/lokit-kelan-vipu-pitkin-hakkuu-957496/ 30.10.2019
        // tukki.SetImage
        peli.Add(tukki);
        return tukki;
    }


    /// <summary>
    /// Aliohjelma kertoo mitä tapahtuu kalan syönnissä. Jos syödään syötävä kala (peruskala tai enemmän pisteitä tuova herkkukala), kala häviää ja pisteet kasvaa.
    /// Jos saukko syö myrkyllisen pallokalan tai jos hai saa kiinni, peli päättyy. Ruutuun tulee teksti pelin päättymisestä, ja pisteiden tallentamisen jälkeen voi siirtyä valikkoon.
    /// </summary>
    void SaukonKalastus(PhysicsObject saukko, PhysicsObject kohde)
    {

        if (kohde.Tag.ToString() == syotavaKalaTag)
        {
            kohde.Destroy();
            pisteLaskuri.Value += 100;
            PisteetLisaaHaita(pisteLaskuri, saukko, haiTag);
        }

        if (kohde.Tag.ToString() == herkkuKalaTag)
        {
            kohde.Destroy();
            pisteLaskuri.Value += 500;
            PisteetLisaaHaita(pisteLaskuri, saukko, haiTag);
        }

        if (kohde.Tag.ToString() == myrkkyKalaTag || kohde.Tag.ToString() == haiTag)
        {
            Valikko();
            ParhaatPisteet();
        }

    }


    /// <summary>
    /// Aliohjelma luo uuden hain kentälle, kun pisteitä kerätty tietty määrä.
    /// </summary>
    void PisteetLisaaHaita(IntMeter pisteLaskuri, PhysicsObject saukko, string haiTag)
    {
        if (pisteLaskuri.Value % 2000 == 0)
        {
            BoundingRectangle alaosa = new BoundingRectangle(new Vector(Level.Left, 0), Level.BoundingRect.BottomRight);
            Kala(haiTag, saukko, 140, 80, haiKuva);  
        }

    }


    /// <summary>
    /// Aliohjelma kerää ja laskee pisteet, joita saa kalojen syömisestä. Pisteet näkyvät oikeassa yläkulmassa.
    /// </summary>
    IntMeter pisteLaskuri;
    void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Right - 80;
        pisteNaytto.Y = Screen.Top - 30;
        pisteNaytto.TextColor = Color.DarkBlue;
        pisteNaytto.Color = Color.LightGray;
        pisteNaytto.Title = "Pisteet";
        pisteLaskuri.MaxValue = 20000;
        pisteLaskuri.UpperLimit += KaikkiKeratty;

        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }

    /// <summary>
    /// Aliohjelmaan siirrytään, jos maksimipisteet tulevat täyteen. Ruutuun tulee teksti.
    /// </summary>
    void KaikkiKeratty()
    {
        Label tekstikentta = new Label("Onnea, olet järven paras kalastajasaukko! Tallenna pisteesi painamalla välilyöntinäppäintä.");
        Add(tekstikentta);
    }


    /// <summary>
    /// Aloittaa pelin. Tullaan tähän, kun pelissä valikossa klikataan kohtaa Aloita uusi peli.
    /// </summary>
    void AloitaPeli()
    {
        ClearAll();
        LuoKentta();
    }


    /// <summary>
    /// Aliohjelma pisteiden tallennukseen. Pelin päättyessä tai voitettaessa siirrytään.
    /// </summary>
    void ParhaatPisteet()
    {

        if (pisteLaskuri.Value == 0)
        {
            Label tekstikentta = new Label("Hävisit pelin!");
            tekstikentta.X = Screen.Left + 500;
            tekstikentta.Y = Screen.Top - 80;
            Add(tekstikentta);
            Valikko();
        }

        if (pisteLaskuri.Value > 0)
        {
            Label tekstikentta = new Label("Hävisit pelin!");
            tekstikentta.X = Screen.Left + 500;
            tekstikentta.Y = Screen.Top - 80;
            Add(tekstikentta);
            topLista.EnterAndShow(pisteLaskuri.Value);
            topLista.HighScoreWindow.Closed += delegate { Valikko(); };
            
        }

    }


    /// <summary>
    /// Pelin alussa valikko, jossa klikataan joko Aloita uusi peli tai Lopeta peli.
    /// </summary>
    void Valikko()
    {
        ClearAll(); 

        Label tekstikentta = new Label(700.0, 600.0, "Syö kaloja, mutta älä syö pallokaloja. Älä anna hain napata." +
            " Saukolla on 45 s aikaa sukeltaa, kunnes on käytävä maalla haukkaamassa happea.");
        tekstikentta.SizeMode = TextSizeMode.Wrapped;
        tekstikentta.TextColor = Color.BloodRed;
        tekstikentta.X = Screen.Left + 550;
        tekstikentta.Y = Screen.Top - 200;
        Add(tekstikentta);


        List<Label> valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Aloita uusi peli");
        kohta1.Position = new Vector(0, 00);
        valikonKohdat.Add(kohta1);

        Label kohta2 = new Label("Parhaat pisteet");
        kohta2.Position = new Vector(0, -40);
        valikonKohdat.Add(kohta2);

        Label kohta3 = new Label("Lopeta peli");
        kohta3.Position = new Vector(0, -80);
        valikonKohdat.Add(kohta3);


        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, AloitaPeli, null);
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, NaytaPisteet, null);
        Mouse.ListenOn(kohta3, MouseButton.Left, ButtonState.Pressed, Exit, null);

    }


    /// <summary>
    /// Jos valikossa painetaan labelia "Parhaat pisteet" tullaan tähän, jossa voi vain katsella pisteitä.
    /// </summary>
    void NaytaPisteet()
    {
        topLista.Show();
    }


    /// <summary>
    /// Saukon hapen riittämisen kertova ajastin (sekunteina). Saukon on käytävä maalla haukkaamassa happea 45 sekunnin välein. 
    /// Jos saukko on maalla ajastimen mennessä nollaan, peli jatkuu. 
    /// Mikäli saukko on vedessä, happi ei riittänyt ja peli päättyy.
    /// </summary>
    DoubleMeter alaspainLaskuri;
    Jypeli.Timer aikaLaskuri;

    void LuoHappiaikaLaskuri(PhysicsObject saukko)
    {
        alaspainLaskuri = new DoubleMeter(45);

        aikaLaskuri = new Jypeli.Timer();
        aikaLaskuri.Interval = 0.1;
        aikaLaskuri.Timeout += delegate { LaskeAlaspain(saukko); aikaLaskuri.Start(); };
        aikaLaskuri.Start(451);

        Label aikaNaytto = new Label();
        aikaNaytto.X = Screen.Left + 80;
        aikaNaytto.Y = Screen.Top - 30;
        aikaNaytto.Color = Color.LightGray;
        //aikaNaytto.Title = "Happi riittää sekunteina";
        aikaNaytto.TextColor = Color.Red;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.BindTo(alaspainLaskuri);
        Add(aikaNaytto);
    }


    /// <summary>
    /// Laskuri laskee aikaa alkaen 45 s kohti 0 s.
    /// </summary>
    void LaskeAlaspain(PhysicsObject saukko)
    {
        alaspainLaskuri.Value -= 0.1;


        if (alaspainLaskuri.Value <= 0)
        {
            MessageDisplay.Add("Aika loppui...");
            // aikaLaskuri.Stop();

            if (saukko.Y < 300)
            {
                Valikko();
                ParhaatPisteet();
            }

            if (saukko.Y >= 300)
            { 
                LuoHappiaikaLaskuri(saukko);
            }

        }

    }


    /// <summary>
    /// Luo uusia kaloja tietyn ajan välein.
    /// </summary>
    void LuoLaskuriKaloille(BoundingRectangle alaosa, PhysicsObject saukko, string syotavaKalaTag, string herkkuKalaTag)
    {
        Jypeli.Timer ajastinKalat = new Jypeli.Timer();

        ajastinKalat.Interval = 5;
        ajastinKalat.Timeout += delegate
        {
            Kala(syotavaKalaTag, saukko, RandomGen.NextDouble(10, 60), RandomGen.NextDouble(10, 30), perusKalaKuva); Kala(herkkuKalaTag, saukko, RandomGen.NextDouble(50, 60), RandomGen.NextDouble(20, 30), herkkuKalaKuva); };

        ajastinKalat.Start(1000);
    }


    /// <summary>
    /// Luo uusia pallokaloja tietyn ajan välein.
    /// </summary>
    void LuoLaskuriPallokaloille(BoundingRectangle alaosa, PhysicsObject saukko, string myrkkyKalaTag)
    {
        Jypeli.Timer ajastinPallokalat = new Jypeli.Timer();

        ajastinPallokalat.Interval = 12;
        ajastinPallokalat.Timeout += delegate { Kala(myrkkyKalaTag, saukko, 50, 40, palloKalaKuva); };

        ajastinPallokalat.Start(1000);

    }

}

        
    



