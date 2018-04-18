# update-user-clients
Mass-update user clients

## Usage

To run this, you’ll need to include two text files in the same directory as the executable:
*	`users.txt` – this file should contain the Artifact IDs of all the users, each on its own line
*	`client.txt` – this file should contain the Artifact ID of the new client you want to associate the users with. It should have just one line.

The output will be logged in a file called `log.txt`. 

One tip: the URL of your instance should not contain `/Relativity` at the end. So it should be something like `https://my-instance`, not `https://my-instance/Relativity`. 
