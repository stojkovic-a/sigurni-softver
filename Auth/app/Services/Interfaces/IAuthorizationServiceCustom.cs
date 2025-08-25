namespace Auth.app.Services.Interfaces;

public interface IAuthorizationServiceCustom
{
    public Task<TestReply> TestFunction(TestMessage tm);
}
