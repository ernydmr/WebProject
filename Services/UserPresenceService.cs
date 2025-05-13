namespace WebProject.Services;

public class UserPresenceService
{
	private readonly HashSet<string> _onlineUsers = new HashSet<string>();

	public void SetOnline(string userId)
	{
		_onlineUsers.Add(userId);
	}

	public void SetOffline(string userId)
	{
		_onlineUsers.Remove(userId);
	}

	public bool IsOnline(string userId)
	{
		return _onlineUsers.Contains(userId);
	}
}