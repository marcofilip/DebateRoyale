# 🗣️ Debate Royale 👑

[![Stato Deploy Somee](https://img.shields.io/website?down_message=OFFLINE&label=debateroyale.somee.com&up_message=ONLINE&url=https%3A%2F%2Fdebateroyale.somee.com)](https://debateroyale.somee.com/)

**📌 Nota Importante:** Stai visualizzando il README del branch `production`. Questo branch è configurato per il **deploy su Somee.com utilizzando SQL Server**. Per la versione di sviluppo locale principale che utilizza **SQLite**, fai riferimento al branch `main`.

## 📄 Descrizione Breve

Debate Royale è un'applicazione web sviluppata in ASP.NET Core 6.0 che simula un gioco di dibattito competitivo 1 contro 1 in tempo reale. Gli utenti possono entrare in stanze virtuali, sfidarsi su argomenti casuali, e venire giudicati sia dagli altri utenti spettatori che da un'intelligenza artificiale (Google Gemini).

## ✨ Funzionalità Principali

-   🔐 **Autenticazione Utenti:** Registrazione e Login sicuri tramite ASP.NET Core Identity.
-   🏠 **Stanze Multiple:** Diverse stanze tematiche dove gli utenti possono radunarsi (6 stanze, limite 20 utenti/stanza).
-   🚪 **Ingresso/Uscita Stanze:** Possibilità per gli utenti loggati di entrare e uscire dalle stanze.
-   🗳️ **Voto Inizio Partita:** Gli utenti in una stanza possono votare per iniziare un round di dibattito.
-   🎲 **Matchmaking Casuale:** Selezione casuale di due giocatori (1v1) dalla stanza all'inizio di ogni round.
-   ⏲️ **Dibattito a Tempo:** Chat dedicata per i due sfidanti per argomentare su un topic casuale (durata 3 minuti).
-   👀 **Voto Spettatori Real-Time:** Gli altri utenti nella stanza possono votare per il giocatore che ritengono stia argomentando meglio.
-   🤖 **Giudizio AI:** Al termine del round, la trascrizione della chat viene inviata all'API di Google Gemini per un'analisi e la dichiarazione di un vincitore.
-   ⚖️ **Punteggio Ibrido:** Il risultato finale del round è determinato da una combinazione ponderata del voto degli spettatori e del verdetto dell'AI.
-   🏆 **Sistema di Punti:** Il vincitore del round ottiene un punto.
-   💻 **Interfaccia Moderna:** Frontend basato su Bootstrap 5 personalizzato con design moderno e fluido.

## 🛠️ Stack Tecnologico

-   **Backend:** C# / ASP.NET Core 6.0
-   **Frontend:** Razor Pages, HTML, CSS, JavaScript
-   **UI Framework:** Bootstrap 5 (personalizzato)
-   **Database (Branch `production`):** SQL Server (per deploy)
-   **Database (Branch `main`):** SQLite (per sviluppo locale)
-   **ORM:** Entity Framework Core 6.0.x
-   **Autenticazione:** ASP.NET Core Identity
-   **Intelligenza Artificiale:** Google Gemini API (via libreria `Mscc.GenerativeAI`)
-   **Real-time:** SignalR
-   **Hosting (Branch `production`):** Somee.com (Free Tier)

## 📋 Prerequisiti (per eseguire il branch `production` localmente)

-   [🟣 .NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
-   Un IDE come [🟦 Visual Studio 2022](https://visualstudio.microsoft.com/) o [🟪 VS Code](https://code.visualstudio.com/)
-   **Un'istanza di SQL Server** installata e accessibile localmente (es. SQL Server Express, SQL Server Developer Edition, o LocalDB installato con Visual Studio).
-   (Opzionale) 🟧 Git per il controllo versione

## ⚙️ Configurazione Iniziale (per eseguire il branch `production` localmente)

1.  **Clona il Repository e passa al branch `production`:**

    ```bash
    git clone https://github.com/marcofilip/DebateRoyale
    cd DebateRoyale
    git checkout production
    ```

2.  **Configura i Segreti Locali (User Secrets):**

    -   Assicurati di avere il .NET SDK installato.
    -   Naviga nella directory del progetto `DebateRoyale` nel tuo terminale.
    -   Inizializza user secrets (solo la prima volta):
        ```bash
        dotnet user-secrets init
        ```
    -   Imposta la tua Gemini API key:
        ```bash
        dotnet user-secrets set "GeminiApiKey" "LA_TUA_API_KEY_PERSONALE_QUI"
        ```
    -   Imposta la Connection String per il tuo SQL Server locale (adatta l'esempio alla tua configurazione!):

        ```bash
        # Esempio per SQL Server LocalDB (comune con Visual Studio)
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=DebateRoyale_ProdBranch;Trusted_Connection=True;MultipleActiveResultSets=true"

        # Esempio per SQL Server Express
        # dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\\SQLEXPRESS;Database=DebateRoyale_ProdBranch;Trusted_Connection=True;MultipleActiveResultSets=true"
        ```

    -   Imposta le credenziali per l'utente Admin iniziale (sarà creato al primo avvio se il DB è vuoto):
        ```bash
        dotnet user-secrets set "AdminUser:Username" "admin_locale"
        dotnet user-secrets set "AdminUser:Email" "admin_locale@example.com"
        dotnet user-secrets set "AdminUser:Password" "UNA_PASSWORD_SICURA_PER_LOCALE"
        ```
    -   Puoi verificare i segreti impostati con: `dotnet user-secrets list`

3.  **Ripristina Dipendenze e Tool:**
    ```bash
    dotnet restore
    dotnet tool restore # Ripristina dotnet-ef se definito in dotnet-tools.json
    ```
4.  **Applicare le Migrazioni del Database (SQL Server):**
    -   Assicurati che il tuo server SQL Server locale sia in esecuzione.
    -   Aprire la Console di Gestione Pacchetti (PMC) in Visual Studio (`Strumenti` > `Gestore pacchetti NuGet` > `Console di Gestione Pacchetti`). Assicurarsi che il progetto predefinito sia `DebateRoyale`. Eseguire:
        ```powershell
        Update-Database
        ```
    -   _Oppure_ da terminale, nella cartella del progetto:
        ```bash
        dotnet ef database update
        ```
    -   Questo creerà/aggiornerà il database `DebateRoyale_ProdBranch` (o come l'hai chiamato nella connection string) sul tuo server SQL locale con lo schema necessario.

## 🚀 Avvio dell'Applicazione (Locale, Branch `production`)

-   **Da Visual Studio:** Premere `F5` o il pulsante "Play" (con profilo `DebateRoyale`).
-   **Da Terminale:** Navigare nella cartella principale del progetto ed eseguire: `dotnet run`

L'applicazione sarà accessibile solitamente su `https://localhost:PORTA` o `http://localhost:PORTA` (controllare l'output del terminale) e si connetterà al tuo database SQL Server locale.

## ☁️ Deployment (Somee.com)

-   Questo branch (`production`) è configurato per il deploy automatico o manuale (via FTP/WebDeploy) su Somee.com.

## 🎮 Come Giocare

1.  Avviare l'applicazione (localmente o visitare l'URL live).
2.  **Registrarsi** per creare un nuovo account.
3.  Effettuare il **Login**.
4.  Navigare alla sezione **"Stanze"** dalla barra di navigazione.
5.  Scegliere una stanza con posti disponibili e cliccare **"Entra"**.
6.  Una volta nella stanza:
    -   Attendere altri giocatori.
    -   Votare per iniziare un round quando richiesto.
7.  Se si viene scelti per il dibattito:
    -   Argomentare nella chat dedicata entro il tempo limite (3 min).
8.  Se si è spettatori:
    -   Seguire il dibattito.
    -   Votare in tempo reale per il giocatore preferito.
9.  Al termine del round, visualizzare il giudizio dell'AI e il vincitore.
10. 🔄 Il ciclo riprende con un nuovo matchmaking.
