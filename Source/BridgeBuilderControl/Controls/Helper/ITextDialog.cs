using System.Threading.Tasks;

namespace BridgeBuilderControl.Controls.Helper
{
    internal interface ITextDialog
    {
        //Zeigt ein Dialog an, wo ein Text zurück gegeben wird
        Task<string> ShowDialogWithTextAnswer(string inputText);
    }
}
