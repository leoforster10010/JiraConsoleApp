# JiraConsoleApp-README #

  
  

### Set-Up ###

* Dateien herunterladen

* JiraConsoleApp.exe im gewünschten Release-Ordner ausführen

* Anweisungen der Anwendung befolgen


### Wichtige Dateien (werden erst nach dem Start der Anwendung erstellt) ###

* JCA_Settings.json: Einstellungs-Datei

* JiraConsoleAppLog.txt: Log-Datei


### Syntax ###

* Groß-/ Kleinschreibung egal
	
* Generelle Syntax: "{Befehl} {Parameter1} {Parameter2}"
	* Bsp:  
		* "assign TEST-123 max.mustermann" => weist das Ticket TEST-123 dem User max.mustermann zu
		* "as 123 m.m" => weist das Ticket 123 des aktuellen Projektes dem ersten User, wessen Kürzel "m.m" entspricht zu, hier wird zusätzlich die Abkürzung des Assign-Befehls verwendet
		* "as TEST-123 n" => hier wird der Bearbeiter von TEST-123 durch die Verwendung des NotAssignedParameters auf "nicht zugewiesen" gestellt
		* "as TEST-123" => weist das Ticket TEST-123 dem eigenen Benutzer zu
	
* Tickets: Tickets werden anhand des aktuellen Projekt-Kürzels vervollständigt, das bedeutet, dass "123" zu "TEST-123" wird, sollte das aktuelle Projekt TEST sein.

* Usernames: Die Kürzel von Benutzernamen werden ebenfalls vervollständigt, d.h. "m.m" wird zu "max.mustermann", sofern "max.mustermann" ein User des aktuelle Jira-Projektes ist


### Befehle ###


* AssignCommand("assign", "as"):
	* Bsp: "assign TEST-123 max.mustermann" / "as 123 m.m"
	* Parameter: (Ticket + Username/ NotAssignedParameter/ *empty*):
		* Username: Weist das Ticket dem entsprechenden User zu
		- NotAssignedParameter: Stellt den User des Tickets auf "nicht zugewiesen"
		- *empty*: Weist das Ticket dem eigenen User zu

* ChangeProjectCommand("change", "cd", "c"):
	* Bsp: "change TEST"
	* Parameter: (Projekt)
		* Wechselt zum Projekt des angegebenen Projekt-Kürzels

* CheckInCommand("checkin", "ci"):
	* Bsp: "checkin TEST-123" / "ci 123"
	* Parameter: (ForceParameter)
		* Beginn der Arbeit an einem Ticket, d.h. ab diesem Zeitpunkt wird die Arbeitszeit gemessen; weist ggf. das Ticket dem eigenen User zu
		* ForceParameter: (...); weist ggf. das Ticket dem eigenen User *ohne Rückfragen* zu

* CheckOutCommand("checkout", "co"):
	* Bsp: "co" / "checkout 2h" / "checkout discard"
	* Parameter: (Arbeitszeit/ DisposeParameter/ *empty*)
		* Arbeitszeit: Beendet die Arbeit am aktuellen Ticket und protokolliert die *angegebene* Arbeitszeit
		* DisposeParameter: Beendet die Arbeit am aktuellen Ticket und protokolliert keine Arbeitszeit
		* *empty*: Beendet die Arbeit am aktuellen Ticket und protokolliert die *gemessene* Arbeitszeit

* DeleteIssueCommand("delete", "d"):
	* Bsp: "delete TEST-123" / "d 123"
	* Parameter: (Ticket)
	* Löscht das angegebene Ticket

* ExitCommand("exit", "x", "quit", "q"):
	* Bsp: "exit" / "x f"
	* Parameter: (ForceParameter)
		* Beendet die Anwendung
		* ForceParameter: Beendet die Anwendung ohne Rückfragen

* HelpCommand("help", "h"):
	* Bsp: "help"
	* Listet alle verfügbaren Befehle auf

* ListProjectIssuesCommand("lsp"):
	* Bsp: "lsp" / "lsp max.mustermann"
	* Parameter: (Username)
		* Listet alle Tickets des aktuellen Projektes auf
		* Username: Listet alle Tickets des angegebenen Users im aktuellen Projekt auf

* ListUserIssuesCommand("list", "ls"):
	* Bsp: "list" / "ls TEST"
	* Parameter: (Projekt)
		* Listet alle Tickets des eigenen Users auf
		* Projekt: Listet alle Tickets des eigenen Users im angegebenen Projekt auf

* LogOutCommand("logout", "lo"):
	* Bsp: "logout" / "lo f"
	* Parameter: (ForceParameter)
		* Meldet den User ab
		* ForceParameter: Meldet den User ohne Rückfragen ab

* MoveIssueCommand("move", "m"):
	* Bsp: "move TEST-123" / "m 123 1" / "m 123 Review"
	* Parameter: (Ticket + Status)
		* Gibt alle verfügbaren Workflow-Optionen des Tickets aus und ändert den Status des Tickets entsprechend der Auswahl.
		* Status: Ändert den Status des Tickets zum angegebenen Status

* OpenCommand("open", "o"):
	* Bsp: "open" / "o TEST-123"
	* Parameter: (Ticket)
		* Öffnet derzeit alle gelisteten Tickets im Browser
		* Ticket: Öffnet das angegebene Ticket im Browser

* SettingsCommand("settings", "s"):
	* Bsp: "settings" / "s Project TEST" 
	* Parameter: (Einstellungsname + Einstellungswert)
		* öffnet die aktuell hinterlegte Einstellungsdatei im Texteditor
		* Einstellungsname + Einstellungswert: Hinterlegt bei der angegebenen Einstellung die übergebenen Werte

* TestCommand("test"):
	* derzeit nicht belegt

* WorklogCommand("worklog", "w", "wl"):
	* Bsp: "worklog TEST-123 1h" / "wl 123 1h"
	* Parameter: (Ticket + Arbeitszeit)
	* Protokolliert bei dem entsprechenden Ticket die angegebene Arbeitszeit


### Parameter ###

- DisposeParameter : "dispose", "discard", "d"
- ForceParameter: "force", "f"
- NotAssignedParameter: "nichtzugewiesen", "n", "notassigned", "na"
  

### Common Errors ###

* Login-Authentification schlägt trotz korrekten Passworts fehl

    => Jira verlangt eine Captcha-Abfrage, d.h. man muss sich im Browser erneut bei Jira anmelden und die Captcha-Abfrage verifizieren


### Useful Links ###

* [Atlassian.Net SDK](https://bitbucket.org/farmas/atlassian.net-sdk/src/master/)

* [Learn Markdown](https://bitbucket.org/tutorials/markdowndemo)


