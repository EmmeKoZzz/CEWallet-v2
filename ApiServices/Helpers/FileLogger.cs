namespace ApiServices.Helpers;

public static class FileLogger {
	
	public static async Task Log(string message) {
		await using var writer = new StreamWriter("../Logs.txt", true);
		await writer.WriteLineAsync($"{DateTime.Now} - {message}:");
		writer.Close();
	}
	
}