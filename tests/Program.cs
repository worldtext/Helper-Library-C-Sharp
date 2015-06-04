using System;
// using WorldText.SMSInterface;

namespace WorldText.SMSInterface
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			// Nested classes seem mostly pointless when public!

			// SMSMessage object = sms/send
			try {
				Outgoing.AccountNumber = "nnnnn";		// Replace with your account ID
				Outgoing.APIKey = "xxxxxxxxxx";			// Replace with your API key

				Outgoing.SMS.SMSMessage sms = new Outgoing.SMS.SMSMessage();

				sms.Destination = "077777111111";		// Replace with destination phone #
				sms.Text = "hello world from client contributed C# code.";
				sms.Send();
			} catch (Exception ex) {
				Console.WriteLine ("Exception caught!");
			}
		}
	}
}
