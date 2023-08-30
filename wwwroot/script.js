document.addEventListener('DOMContentLoaded', function () {
    const buttons = document.querySelectorAll('.buttons button');
    buttons.forEach(button => {
        button.addEventListener('click', async function () {
            const command = this.name;
            console.log('Sending command:', command);
            responseText.textContent = 'Sending command...';
            const apiUrl = '/api/Grbl'

            try {
                const response = await fetch(apiUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    
                    body: JSON.stringify(command)
                });

                const data = await response.text();
                responseText.textContent = data;
            } catch (error) {
                console.error('Error sending command:', error);
                responseText.textContent = 'Error sending command.';
            }
        });
    });
});
document.getElementById('fileUploadForm').addEventListener('submit', async function (event) {
    event.preventDefault(); // Prevent form submission

    const fileInput = document.querySelector('input[type="file"]');
    const file = fileInput.files[0];

    if (file) {
        const reader = new FileReader();
        reader.onload = async function (e) {
            const content = e.target.result;
            const lines = content.split('\n');

            for (const line of lines) {
                // Perform your POST API call for each line
                if (line.trim() !== '') {
                    try {
                        if(document.getElementById('state').innerHTML=='ok') {
                        await postDataWithResponseCheck(line, 1000);
                        }
                        else {
                            console.error('Error making API call:', error);
                            break;
                        } // Delay of 1 second (1000 ms)
                    } catch (error) {
                        console.error('Error making API call:', error);
                        break;
                        // Stop sending commands if API call fails
                    }
                }
            }
        };

        reader.readAsText(file);
    }
});
async function postDataWithResponseCheck(data, delay) {
    return new Promise(async (resolve, reject) => {
        let responseReceived = false;


        setTimeout(async () => {
            try {
                await postDataToApi(data);
                responseReceived = true;
                resolve();
            } catch (error) {
                reject(error);
            }
        }, delay);

        // Poll for responseReceived status before proceeding to the next command
        const pollInterval = 100; // Polling interval in milliseconds
        const maxPollAttempts = 30; // Maximum number of polling attempts
        let pollCount = 0;

        const poll = setInterval(() => {
            if (responseReceived || pollCount >= maxPollAttempts) {
                clearInterval(poll);
                if (!responseReceived) {
                    reject(new Error(`API response not received for command: ${data}`));
                }
            }
            pollCount++;
        }, pollInterval);
    });
}

async function postDataToApi(data) {
    const command = data;
    console.log('Sending command:', command);
    const apiUrl = '/api/Grbl'

    try {
        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },

            body: JSON.stringify(command)
        });
        const data = await response.text();

console.log(data);
        if (data!=('ok')) {
            document.getElementById('state').innerHTML = 'error';
            throw new Error(`API request failed for command: ${data}`);
        }
    
    } catch (error) {
        console.error('Error sending command:', error);
        responseText.textContent = 'Error sending command.';
    }

    
}

