# Debate Royale

**Stato del Progetto:** In Sviluppo 🚧

## Descrizione Breve

Debate Royale è un'applicazione web sviluppata in ASP.NET Core 6.0 che simula un gioco di dibattito competitivo 1 contro 1 in tempo reale. Gli utenti possono entrare in stanze virtuali, sfidarsi su argomenti casuali, e venire giudicati sia dagli altri utenti spettatori che da un'intelligenza artificiale (Google Gemini).

## Funzionalità Principali (Previste)

*   **Autenticazione Utenti:** Registrazione e Login sicuri tramite ASP.NET Core Identity.
*   **Stanze Multiple:** Diverse stanze tematiche dove gli utenti possono radunarsi (limite 50 utenti/stanza).
*   **Ingresso/Uscita Stanze:** Possibilità per gli utenti loggati di entrare e uscire dalle stanze.
*   **Voto Inizio Partita:** Gli utenti in una stanza possono votare per iniziare un round di dibattito.
*   **Matchmaking Casuale:** Selezione casuale di due giocatori (1v1) dalla stanza all'inizio di ogni round.
*   **Dibattito a Tempo:** Chat dedicata per i due sfidanti per argomentare su un topic casuale (durata 1 minuto).
*   **Voto Spettatori Real-Time:** Gli altri utenti nella stanza possono votare per il giocatore che ritengono stia argomentando meglio.
*   **Giudizio AI:** Al termine del round, la trascrizione della chat viene inviata all'API di Google Gemini per un'analisi e la dichiarazione di un vincitore.
*   **Punteggio Ibrido:** Il risultato finale del round è determinato da una combinazione ponderata (50/50) del voto degli spettatori e del verdetto dell'AI.
*   **Sistema di Punti:** Il vincitore del round ottiene un punto (meccanismo di classifica futuro).
*   **Interfaccia Moderna:** Frontend basato su Bootstrap 5 personalizzato con design moderno e fluido.

## Stack Tecnologico

*   **Backend:** C# / ASP.NET Core 6.0
*   **Frontend:** Razor Pages, HTML, CSS, JavaScript
*   **UI Framework:** Bootstrap 5 (personalizzato)
*   **Database:** SQLite
*   **ORM:** Entity Framework Core 6.0.x
*   **Autenticazione:** ASP.NET Core Identity
*   **Intelligenza Artificiale:** Google Gemini API (via chiamate HTTP dirette)
*   **Real-time (Pianificato):** SignalR

## Prerequisiti

*   [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
*   Un IDE come [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)
*   (Opzionale) Git per il controllo versione

## Configurazione Iniziale

1.  **Clonare il Repository (se applicabile):**
    ```bash
    git clone <url-repository>
    cd DebateRoyale
    ```
2.  **Configurare i Segreti Utente:**
    *   Aprire il gestore dei segreti utente per il progetto (in Visual Studio: Tasto destro sul progetto -> Gestisci segreti utente).
    *   Assicurarsi che il file `secrets.json` contenga:
        ```json
        {
          "Gemini": {
            "ApiKey": "LA_TUA_CHIAVE_API_GEMINI_QUI"
          },
          "ConnectionStrings": {
            "DefaultConnection": "DataSource=app.db;Cache=Shared"
          }
        }
        ```
    *   **Importante:** Sostituire `"LA_TUA_CHIAVE_API_GEMINI_QUI"` con la propria chiave API valida ottenuta da Google AI Studio. **Non committare mai la chiave API nel controllo versione!**
3.  **Ripristinare i Pacchetti NuGet:**
    *   In Visual Studio, la build dovrebbe farlo automaticamente.
    *   Da terminale: `dotnet restore`
4.  **Applicare le Migrazioni del Database:**
    *   Aprire la Console di Gestione Pacchetti (PMC) in Visual Studio (`Strumenti` > `Gestore pacchetti NuGet` > `Console di Gestione Pacchetti`).
    *   Assicurarsi che il progetto predefinito sia `DebateRoyale`.
    *   Eseguire il comando: `Update-Database`
    *   Questo creerà il file `app.db` nella cartella principale con lo schema necessario.

## Avvio dell'Applicazione

*   **Da Visual Studio:** Premere `F5` o il pulsante "Play" (con profilo `DebateRoyale`).
*   **Da Terminale:** Navigare nella cartella principale del progetto ed eseguire: `dotnet run`

L'applicazione sarà accessibile solitamente su `https://localhost:PORTA` o `http://localhost:PORTA` (controllare l'output del terminale).

## Come Giocare (Flusso Previsto)

1.  Avviare l'applicazione.
2.  **Registrarsi** per creare un nuovo account.
3.  Effettuare il **Login**.
4.  Navigare alla sezione **"Stanze"** dalla barra di navigazione.
5.  Scegliere una stanza con posti disponibili e cliccare **"Entra"**.
6.  Una volta nella stanza (pagina `RoomDetail` - da implementare):
    *   Attendere altri giocatori.
    *   Votare per iniziare un round quando richiesto.
7.  Se si viene scelti per il dibattito:
    *   Argomentare nella chat dedicata entro il tempo limite (1 min).
8.  Se si è spettatori:
    *   Seguire il dibattito.
    *   Votare in tempo reale per il giocatore preferito.
9.  Al termine del round, visualizzare il giudizio dell'AI e il vincitore.
10. Il ciclo riprende con un nuovo matchmaking.