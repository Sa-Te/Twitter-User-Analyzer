using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {

        // Prompt the user to input proxy details
        Console.WriteLine("Enter Proxy Address:");
        string proxyAddress = Console.ReadLine()!;

        Console.WriteLine("Enter Proxy Port:");
        int proxyPort;
        while (!int.TryParse(Console.ReadLine(), out proxyPort) || proxyPort <= 0)
        {
            Console.WriteLine("Invalid input for proxy port. Please enter a valid port number:");
        }

        Console.WriteLine("Enter Proxy Username:");
        string proxyUsername = Console.ReadLine()!;

        Console.WriteLine("Enter Proxy Password:");
        string proxyPassword = Console.ReadLine()!;


        Console.WriteLine("Enter the maximum number of threads:");
        string maxThreadsInput = Console.ReadLine()!;

        if (!int.TryParse(maxThreadsInput, out int maxThreads) || maxThreads <= 0)
        {
            Console.WriteLine("Invalid input for maximum number of threads. Exiting program.");
            return;
        }

        // Verify proxy credentials
        bool proxyCredentialsValid = await VerifyProxyCredentials(proxyAddress, proxyPort, proxyUsername, proxyPassword);
        if (!proxyCredentialsValid)
        {
            Console.WriteLine("Proxy credentials are invalid. Exiting program.");
            return;
        }

        // Check network connectivity to the proxy server
        bool proxyServerReachable = await CheckProxyServerReachability(proxyAddress, proxyPort);
        if (!proxyServerReachable)
        {
            Console.WriteLine("Unable to reach the proxy server. Exiting program.");
            return;
        }

        var listFilePath = Path.Combine(Directory.GetCurrentDirectory(), "list.txt");
        // Read lines from the file
        var lines = File.ReadAllLines(listFilePath);

        // Split lines into batches based on the maximum number of threads
        var batches = Batch(lines, maxThreads);

        // Create tasks for each batch
        var tasks = new List<Task>();
        foreach (var batch in batches)
        {
            var task = ProcessBatchAsync(batch, proxyAddress, proxyPort, proxyUsername, proxyPassword);
            tasks.Add(task);
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);
    }
    static async Task<bool> VerifyProxyCredentials(string proxyAddress, int proxyPort, string proxyUsername, string proxyPassword)
    {
        try
        {
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = new WebProxy($"http://{proxyAddress}:{proxyPort}"),
                UseProxy = true,
                Credentials = new NetworkCredential(proxyUsername, proxyPassword)
            };

            using var httpClient = new HttpClient(httpClientHandler);
            // Send a test request to verify credentials
            var response = await httpClient.GetAsync("http://www.example.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    static async Task<bool> CheckProxyServerReachability(string proxyAddress, int proxyPort)
    {
        try
        {
            // Ping the proxy server to check reachability
            using var ping = new System.Net.NetworkInformation.Ping();
            var result = await ping.SendPingAsync(proxyAddress, 1000);
            return result.Status == System.Net.NetworkInformation.IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }


    static async Task ProcessBatchAsync(IEnumerable<string> batch, string proxyAddress, int proxyPort, string proxyUsername, string proxyPassword)
    {
        // Create HttpClient instance with proxy
        var httpClient = CreateHttpClientWithProxy(proxyAddress, proxyPort, proxyUsername, proxyPassword);

        foreach (var line in batch)
        {
            // Process each line in the batch
            await SortAsync(line, httpClient);
        }

        // Dispose HttpClient after processing the batch
        httpClient.Dispose();
    }

    // Sort asynchronous method to process each line
    static async Task SortAsync(string akppm, HttpClient httpClient)
    {
        try
        {
            // Obtain guest token
            var guestToken = await GetGuestToken(httpClient);

            var parts = akppm.Split(":");
            var username = parts[2];

            // Construct URL
            var url = $"https://api.twitter.com/graphql/qW5u-DAuXpMEG0zA1F7UGQ/UserByScreenName?variables=%7B%22screen_name%22%3A%22{username}%22%2C%22withSafetyModeUserFields%22%3Atrue%7D&features=%7B%22hidden_profile_likes_enabled%22%3Atrue%2C%22hidden_profile_subscriptions_enabled%22%3Atrue%2C%22rweb_tipjar_consumption_enabled%22%3Atrue%2C%22responsive_web_graphql_exclude_directive_enabled%22%3Atrue%2C%22verified_phone_label_enabled%22%3Afalse%2C%22subscriptions_verification_info_is_identity_verified_enabled%22%3Atrue%2C%22subscriptions_verification_info_verified_since_enabled%22%3Atrue%2C%22highlights_tweets_tab_ui_enabled%22%3Atrue%2C%22responsive_web_twitter_article_notes_tab_enabled%22%3Atrue%2C%22creator_subscriptions_tweet_preview_api_enabled%22%3Atrue%2C%22responsive_web_graphql_skip_user_profile_image_extensions_enabled%22%3Afalse%2C%22responsive_web_graphql_timeline_navigation_enabled%22%3Atrue%7D&fieldToggles=%7B%22withAuxiliaryUserLabels%22%3Afalse%7D";

            // Make request to Twitter API
            var jsonResponse = await MakeRequest(httpClient, url, guestToken);

            // Write to appropriate files based on response
            await WriteToFile(parts, jsonResponse);
        }
        catch (Exception e)
        {
            // Log error and write line to skip file
            Console.WriteLine($"An error occurred for {akppm}: {e}");
            await WriteToSkipFile(akppm);
        }
    }

    // Get guest token for authentication
    static async Task<string?> GetGuestToken(HttpClient httpClient)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/1.1/guest/activate.json");
        request.Headers.Add("Host", "api.twitter.com");
        if (request.Content != null)
        {
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }
        request.Headers.Add("authorization", "Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
        request.Headers.Add("Accept", "*/*");

        request.Content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(json);
        var guestTokenProperty = jsonDoc.RootElement.GetProperty("guest_token");

        return guestTokenProperty.ValueKind != JsonValueKind.Undefined ? guestTokenProperty.GetString() : null;
    }

    // Make HTTP request to Twitter API
    static async Task<string> MakeRequest(HttpClient httpClient, string url, string? guestToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Host", "api.twitter.com" },
            { "Content-Type", "application/json" },
            { "authorization", "Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA" },
            { "x-guest-token", guestToken ?? "" },
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36" },
            { "Accept", "*/*" }
        };

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    // Write response to appropriate files based on content
    static async Task WriteToFile(string[] parts, string jsonResponse)
    {
        if (jsonResponse.Contains("user"))
        {
            var jsonDoc = JsonDocument.Parse(jsonResponse);

            using var en = new StreamWriter("checked.txt", true);
            await en.WriteLineAsync($"{parts[2]}:{parts[1]}:{parts[0]}:{parts[3]}");

            // Check for verified type and write to corresponding files
            if (jsonResponse.Contains("verified_type"))
            {
                var verifiedType = jsonDoc.RootElement.GetProperty("data").GetProperty("user").GetProperty("result").GetProperty("legacy").GetProperty("verified_type").GetString();
                if (verifiedType == "Business")
                {
                    if (jsonResponse.Contains("affiliates_count"))
                    {
                        using var enGoldKwde = new StreamWriter("gold kwde.txt", true);
                        await enGoldKwde.WriteLineAsync($"{parts[2]}:{parts[1]}:{parts[0]}:{parts[3]}");
                    }
                    else
                    {
                        using var enGold = new StreamWriter("gold.txt", true);
                        await enGold.WriteLineAsync($"{parts[2]}:{parts[1]}:{parts[0]}:{parts[3]}");
                    }
                }
                else if (verifiedType == "Government")
                {
                    using var enGrey = new StreamWriter("grey.txt", true);
                    await enGrey.WriteLineAsync($"{parts[2]}:{parts[1]}:{parts[0]}:{parts[3]}");
                }
            }

            // Check if user is blue verified and write to blue.txt
            if (jsonResponse.Contains("is_blue_verified"))
            {
                var isBlueVerified = jsonDoc.RootElement.GetProperty("data").GetProperty("user").GetProperty("result").GetProperty("is_blue_verified").GetBoolean();
                if (isBlueVerified)
                {
                    using var enBlue = new StreamWriter("blue.txt", true);
                    await enBlue.WriteLineAsync($"{parts[2]}:{parts[1]}:{parts[0]}:{parts[3]}");
                }
            }
        }
        else
        {
            // Write line to acc not found.txt if user data is not found
            using var enAccNotFound = new StreamWriter("acc not found.txt", true);
            await enAccNotFound.WriteLineAsync($"{parts[2]}:{parts[1]}:{parts[0]}:{parts[3]}");
        }
    }

    static async Task WriteToSkipFile(string akppm)
    {
        const int maxRetries = 3;
        const int delayMilliseconds = 100;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using (var en = new StreamWriter("skip.txt", true))
                {
                    await en.WriteLineAsync(akppm);
                }
                // If the write succeeds, break out of the loop
                break;
            }
            catch (IOException)
            {
                // If the file is still in use, wait and retry
                await Task.Delay(delayMilliseconds);
            }
        }
    }


    // Split source into batches of specified size
    static IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> source, int batchSize)
    {
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return YieldBatchElements(enumerator, batchSize - 1);
        }

        IEnumerable<T> YieldBatchElements(IEnumerator<T> enumerator, int batchSize)
        {
            yield return enumerator.Current;
            for (int i = 0; i < batchSize && enumerator.MoveNext(); i++)
            {
                yield return enumerator.Current;
            }
        }
    }

    // Create HttpClient instance with proxy
    static HttpClient CreateHttpClientWithProxy(string proxyAddress, int proxyPort, string proxyUsername, string proxyPassword)
    {
        var httpClientHandler = new HttpClientHandler
        {
            Proxy = new WebProxy($"http://{proxyAddress}:{proxyPort}"),
            UseProxy = true,
            Credentials = new NetworkCredential(proxyUsername, proxyPassword)
        };

        return new HttpClient(httpClientHandler);
    }
}
