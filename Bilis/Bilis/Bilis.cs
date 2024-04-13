using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Mail;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Silk.NET.GLFW;
using MouseButton = Jypeli.MouseButton;
namespace Bilis;

/// @author 
/// @version 03.02.2024
/// <summary>
/// Biljardipeli, Pelin tavoitteena pussittaa kaikki pallot.
/// Ohjauksena, lyönti tapahtuu hiiren vasemmalla painikkeella ja pallon palautus tapahtuu oikealla.
/// </summary>
public class Bilis : PhysicsGame
{
    PhysicsObject seina;
    PhysicsObject pallor;
    PhysicsObject ppallo;
    PhysicsObject lpallo;
    PhysicsObject alaReuna;
    PhysicsObject ylaReuna;
    PhysicsObject pallodelete;
    private bool ppalloolemassa = false;
    private int pallojenMaara = 15;
    private static bool pelikaynnissa = false;
    

 /// <summary>
 /// Aloittaa pelin ja alustaa pelin
 /// </summary>
    public override void Begin()
    {
        //tarkista pelin tila ja aseta poistumismahdollisuudet ja kenttä kuntoon
        if (!pelikaynnissa)
        {
            LuoKentta();
            pelikaynnissa = true;
            PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        }
    }
    
    
    /// <summary>
    /// Voitit pelin, näyttää napin pallojen palauttamisen alkuperäisille paikoilleen.
    /// </summary>
    void voititPelin()
    {
        MultiSelectWindow loppuvalikko = new MultiSelectWindow("Pelin loppuvalikko", "Uusi peli");
        loppuvalikko.AddItemHandler(0, Begin);
        Add(loppuvalikko);
        ppalloolemassa = false;
        pallojenMaara = 15;
        ppallo.Destroy();
        Asetapallot();
        LuoPpallo(Level.Left + 120.0, 30);

    }

    
    /// <summary>
    /// Kentän rajan rakennuspalikka.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    PhysicsObject LuoPalloreuna(double x, double y)
    {
        pallor = new PhysicsObject(width: 40.0, height: 40.0);
        pallor.Shape = Shape.Circle;
        pallor.X = x;
        pallor.Y = y;
        pallor.MaxVelocity = 0.0;
        pallor.CollisionIgnoreFunc = Tormaako;
        pallor.Tag = "reuna";
        pallor.Restitution = 1.0;
        Add(pallor, -1);
        return pallor;
    }
    
    
   /// <summary>
   /// Pallon poistajaelementti. poistaa pallot jotka osuvat siihen.
   /// </summary>
   /// <param name="x"></param>
   /// <param name="y"></param>
   /// <returns></returns>
    PhysicsObject LuoPallopoisto(double x, double y)
    {
        pallodelete = new PhysicsObject (PhysicsObject.CreateStaticObject((width: 30.0, height: 30.0));
        pallodelete.Shape = Shape.Circle;
        pallodelete.Color = Color.Black;
        pallodelete.X = x;
        pallodelete.Y = y;
        pallodelete.Restitution = 0.0;
        pallodelete.MaxVelocity = 0.0;
        pallodelete.IgnoresPhysicsLogics = true;
        pallodelete.CollisionIgnoreFunc = Tormaako;
        pallodelete.Tag = "poistaja";
        Add(pallodelete);
        return pallodelete;
    }
    
/// <summary>
/// Alustaa pelikentän pelin alussa.
/// </summary>
    void LuoKentta()
    {
        // refaktoiroitava loopilla.
        Camera.ZoomToAllObjects();
        // Iteroidaan luo seina metodi
        double[] sijainti = { 60.0, 140.0, 280.0, 380.0 };
        foreach (double paikka in sijainti)
        {
            LuoSeina(Level.Left, paikka);
            LuoSeina(Level.Left + 240.0, paikka);
        }
        
        // Iteroidaan reunapallot
        double[] reunapallonSijainti = { -10.0, 210.0, 450.0 };
        foreach (double rSijainti in reunapallonSijainti)
        {
            LuoPalloreuna(Level.Left, rSijainti);
            LuoPalloreuna(Level.Left + 240.0, rSijainti);
        }
        
        // Iteroidaan Poistopallot
        double[] poistoSijainti = { 5.0, 240.0, 440.0 };
        foreach (double pSijainti in poistoSijainti)
        {
            LuoPallopoisto(Level.Left, pSijainti);
            LuoPallopoisto(Level.Left + 240.0, pSijainti);
        }
        
        // Iteroidaan ala- ja yläseinät
        double[] katto = { -15.0, 460.0 };
        foreach (double kSijainti in katto)
        {
            LuoSeinaya(Level.Left + 70.0, kSijainti);
            LuoSeinaya(Level.Left + 170.0, kSijainti);
        }
        //Luodaan pelikentälle pelivälineet
        Asetapallot();
        LuoPpallo(Level.Left + 120.0, 30);
    }
    

   /// <summary>
   /// Seinäobjekti rajaamaan karttaa
   /// </summary>
   /// <param name="x"></param>
   /// <param name="y"></param>
   /// <returns></returns>
    PhysicsObject LuoSeina(double x, double y)
    {
        seina = PhysicsObject.CreateStaticObject(20.0, 100.0);
        seina.Shape = Shape.Rectangle;
        seina.X = x;
        seina.Y = y;
        seina.Restitution = 1.0;
        Add(seina);
        return seina;
    }
    

    /// <summary>
    /// alaseinä, rajaa ylä ja alareunan.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    PhysicsObject LuoSeinaya(double x, double y)
    {
        seina = PhysicsObject.CreateStaticObject(100.0, 20.0);
        seina.Shape = Shape.Rectangle;
        seina.X = x;
        seina.Y = y;
        seina.Restitution = 1.0;
        Add(seina);
        return seina;
    }
    

    /// <summary>
    /// Pelattava kivi, ohjaimet hiiren painikkeet. vasen lyö oikea palauttaa takaisin.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    PhysicsObject LuoPpallo(double x, double y)
    {
        ppallo = new PhysicsObject(width: 20.0, height: 20.0);
        ppallo.Shape = Shape.Circle;
        ppallo.Color = Color.Crimson;
        ppallo.X = x;
        ppallo.Y = y;
        ppallo.Tag = "pelipallo";
        Mouse.Listen(MouseButton.Left, ButtonState.Pressed, Lyo, "testi");
        ppallo.Restitution = 0.6; //fysiikkamoottorin kimmoisuus
        lpallo.CollisionIgnoreFunc = Tormaako; // Tarkasta törmäykset
        AddCollisionHandler(ppallo, "poistaja", pussitus2); //Pallon poiston tarkastus ja palauttaminen
        Add(ppallo);
        ppalloolemassa = true;
        return ppallo;
    }
    
    
/// <summary>
/// Lyöntifunktio, asettaa nopeuden kivelle.
/// </summary>
    void Lyo()
    {
        Mouse.IsCursorVisible = true;
        double Xa = Mouse.PositionOnWorld.X;
        double Ya = Mouse.PositionOnWorld.Y;
        Vector impulssi = new Vector(Xa - ppallo.X, Ya - ppallo.Y);
        double voima = (Math.Pow(ppallo.X - Xa, 2) + Math.Pow(ppallo.Y - Ya, 2)); //suunta + voima vektorin pituudesta kursori-pallo välillä
        ppallo.Hit(impulssi * voima * 0.00005); //lyönnin skaalaaminen, aiheuttaa ongemia lyhyillä vektoreilla
    }
    

    /// <summary>
    /// Lyötävät pallot, näitä on 15kpl ja peli loppuu kun kaikki on poistettu pelistä.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    PhysicsObject LuoLpallo(double x, double y)
    {
        lpallo = new PhysicsObject(width: 20.0, height: 20.0);
        lpallo.Shape = Shape.Circle;
        lpallo.X = x;
        lpallo.Y = y;
        lpallo.Tag = "pallo";
        lpallo.CollisionIgnoreFunc = Tormaako;
        lpallo.Restitution = 0.6;
        AddCollisionHandler(lpallo, "poistaja", pussitus);
        Add(lpallo);
        return lpallo;
    }
    

    /// <summary>
    /// törmäyksenkäsittely.
    /// </summary>
    /// <param name="eka"></param>
    /// <param name="toka"></param>
    /// <returns></returns>
    private bool Tormaako(IPhysicsObject eka, IPhysicsObject toka)
    {
        if (eka.Tag == "reuna" || toka.Tag == "reuna")
            return false;
        return true;
    }
    

    /// <summary>
    /// Pussitus, poistaa lyötävät pallot pelistä ja tarkastaa jäljelläolevan määrän.
    /// Kutsuu lopuksi Voitit pelin.
    /// </summary>
    /// <param name="lpallo"></param>
    /// <param name="pallodelete"></param>
    public void pussitus(PhysicsObject lpallo, PhysicsObject pallodelete)
    {
        lpallo.Destroy();
        pallojenMaara = pallojenMaara-1;
        Console.WriteLine(pallojenMaara);
        if (pallojenMaara == 0)
        {
            voititPelin();
        }
    }
    
    
    /// <summary>
    /// pussitus pelattavalle, Palauttaa pallon peliin, kursorin sijaintiin. 
    /// </summary>
    /// <param name="ppallo"></param>
    /// <param name="pallodelete"></param>
    public void pussitus2(PhysicsObject ppallo, PhysicsObject pallodelete)
    {
        ppallo.Destroy();
        ppalloolemassa = false;
        if (ppallo.IsDestroyed)
        {
           Mouse.Listen( MouseButton.Right, ButtonState.Down, palautaPallo, "palauttaa pallon peliin");
           
        }
    
    }
    
    
 /// <summary>
 /// Palautta Kiven, sisältään logiikkan pallon palautukselle kursorin sijaintiin, kutsutaan pussitus2 funktiossa.
 /// </summary>
    void palautaPallo()
    {
        if (!ppalloolemassa)
        {
            Mouse.IsCursorVisible = true;
            ppallo = new PhysicsObject(width: 20.0, height: 20.0);
            ppallo.Shape = Shape.Circle;
            ppallo.Color = Color.Crimson;
            ppallo.X = Mouse.PositionOnWorld.X;
            ppallo.Y = Mouse.PositionOnWorld.Y;
            ppallo.Tag = "pelipallo";
            ppallo.Restitution = 0.6;
            AddCollisionHandler(ppallo, "poistaja", pussitus2);
            ppalloolemassa = true;
            Add(ppallo);
            //return ppallo;
        }
    }
    
    /// <summary>
    /// Pallojen asettaminen pelin alkuun. on hoidettu loppu koodissa.
    /// </summary>
    /// <param name="rownumber"></param>
    /// <returns></returns>
    public double[] Rowxs(int rownumber)
    {
        //r on pallon säde
        // lasketaan pallojen x koordinaatit
        const double r = 10;
        return rownumber switch
        {
            0 => new double[] { 0 },
            1 => new double[] { -r, r },
            2 => new double[] { -2 * r, 0, 2 * r },
            3 => new double[] { -3 * r, -r, r, 3 * r },
            4 => new double[] { -4 * r, -2 * r, 0, 2 * r, 4 * r },
            _ => new double[] { }
        };
    }

    // Laskee Y koordinaatit 
    public double Rowy(int rownumber)
    {
        return rownumber * (Math.Sqrt(5) * 10.0);
    }

    // kutsuu loopilla X ja Y koordinaatteja  ja tallentaa koordinaatit listaan.
    // Kutsuttaessa luo pallot kolmiomuodostelmaan
    public PhysicsObject[] Asetapallot()
    {
        
        PhysicsObject[] koordinaatit = new PhysicsObject[pallojenMaara];
        int index = 0;
        for (int rownumber = 0; rownumber < 5; rownumber++)
        {
            double y = Rowy(rownumber);
            foreach (double x in Rowxs(rownumber))
            {
                koordinaatit[index] = LuoLpallo(Level.Left + 120.0 + x, 260 + y);
                index++;
            }
        }

        return koordinaatit;
    }
}