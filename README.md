# Twitter User Analyzer

Twitter User Analyzer is a command-line tool written in C# for analyzing Twitter user profiles. It checks a list of usernames and passwords against the Twitter API to retrieve valuable user information such as verification status, account type, and more.

## Getting Started

1. Clone the repository to your local machine.
2. Open the solution file (`TwitterUserAnalyzer.sln`) in your preferred IDE (e.g., Visual Studio, Visual Studio Code).
3. Install any necessary dependencies.
4. Build the solution.
5. Run the application.

## Usage

1. Upon running the application, you'll be prompted to enter the following information:
   - Proxy Address: The address of the proxy server to be used for accessing the Twitter API.
   - Proxy Port: The port number of the proxy server.
   - Proxy Username: Username for authenticating with the proxy server (if required).
   - Proxy Password: Password for authenticating with the proxy server (if required).
   - Maximum Number of Threads: Specify the maximum number of concurrent threads for processing the user list.
2. Provide the path to the file containing the list of usernames and passwords in the following format:

For example:

**************ys@gmail.com:Rich Posert:PlethoraChutney:1081:Wed May 25 00:43:07 +0000 2011


3. Once the necessary information is provided, the application will start processing the user list.
4. Sit back and wait for the analysis to complete.

## Configuration

- **Proxy Address**: The address of the proxy server to be used for accessing the Twitter API.
- **Proxy Port**: The port number of the proxy server.
- **Proxy Username**: Username for authenticating with the proxy server (if required).
- **Proxy Password**: Password for authenticating with the proxy server (if required).
- **Maximum Number of Threads**: Specify the maximum number of concurrent threads for processing the user list.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
