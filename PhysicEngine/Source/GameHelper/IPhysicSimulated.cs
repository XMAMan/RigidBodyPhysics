namespace GameHelper
{
    //Mit diesen Interface werden die MainViewModel der Spiele testbar. So kann eine aufgezeichnet Simulation abgespielt werden
    //und im Test kann dann geprüft werden, ob die ScreenShoots alle stimmen
    public interface IPhysicSimulated
    {
        int LoadSimulation(string levelFile, string keyboardFile);// Ließt das Level+Keyboard-File ein und überspringt den Intro-Screen; Returnwert: Anzahl der TimerTicks von der KeyBoard-Datei
        bool DoTimeStep(float dt); //Return: KeyboardPlayer.IsFinish
    }
}
