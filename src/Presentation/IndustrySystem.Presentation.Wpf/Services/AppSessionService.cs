namespace IndustrySystem.Presentation.Wpf.Services;

public class AppSessionService : IAppSessionService
{
    private readonly IAuthState _authState;

    public AppSessionService(IAuthState authState)
    {
        _authState = authState;
    }

    public void LogoutAndShowLoginDialog()
    {
        _authState.SignOut();

        if (System.Windows.Application.Current is not App app)
        {
            return;
        }

        if (app.MainWindow is Shell shell)
        {
            shell.PrepareForLogout();
            shell.Close();
        }

        app.ShowLoginDialog();
    }
}
