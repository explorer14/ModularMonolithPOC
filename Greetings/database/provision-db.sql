IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'GreetingsDB')
BEGIN
    CREATE DATABASE [GreetingsDB]
END