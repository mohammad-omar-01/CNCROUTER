document.addEventListener('DOMContentLoaded', function () {


    const buttons = document.querySelectorAll('.buttons .Control');

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
const uploadForm = document.getElementById('uploadForm');
const fileInput = document.getElementById('fileInput');
let started=false;
uploadForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    const file = fileInput.files[0];

    if (!file) {
        console.error('No file selected.');
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await fetch('/api/grbl/upload', {
            method: 'POST',
            body: formData,
        });

        if (response.ok) {
            const result = await response.text();
            console.log('File uploaded successfully:', result);
started=true;
        } else {
            console.error('File upload failed:', response.statusText);
        }
    } catch (error) {
        console.error('An error occurred:', error);
    }
});

const stop = document.getElementById('Stop').addEventListener('click', async function () {
    const command = this.name;
    console.log('Sending command:', command);
    responseText.textContent = 'Sending command...';
    const apiUrl = '/api/Grbl/Stop';

    try {
        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
        });
        const data = await response.text();
        responseText.textContent = data;
    } catch (error) {
        console.error('Error sending command:', error);
        responseText.textContent = 'Error sending command.';
    }
});
const Pause = document.getElementById('Pause').addEventListener('click', async function () {
    const command = this.name;
    console.log('Sending command:', command);
    responseText.textContent = 'Sending command...';
    const apiUrl = '/api/Grbl/Pause';

    try {
        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
        });
        const data = await response.text();
        responseText.textContent = data;
    } catch (error) {
        console.error('Error sending command:', error);
        responseText.textContent = 'Error sending command.';
    }
});
const Continue = document.getElementById('Continue').addEventListener('click', async function () {
    const command = this.name;
    console.log('Sending command:', command);
    responseText.textContent = 'Sending command...';
    const apiUrl = '/api/Grbl/Continue';

    try {
        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
        });
        const data = await response.text();
        responseText.textContent = data;
    } catch (error) {
        console.error('Error sending command:', error);
        responseText.textContent = 'Error sending command.';
    }
});
const percentage = document.getElementById('percentage');

function updatePercentage() {
    fetch('/api/Grbl/percentage')
        .then(response => response.text())
        .then(data => {
            const progress = parseFloat(data);

            // Format the progress with two digits after the dot
            const formattedProgress = progress.toFixed(2);

            percentage.textContent = `Current Progress: ${formattedProgress}%`;

        })
        .catch(error => {
            console.error('Error fetching data:', error);
        });

    // Call the updatePercentage function again after 30 seconds
    setTimeout(updatePercentage, 5000); // 30 seconds in milliseconds
}
if(clicked)
setInterval(fetchSensorStatus, 5000);


const lastStatuses = [];
let statusCounter = 0;
const nStatuses = 5;
function fetchSensorStatus() {
    fetch('/api/Grbl/Status')
        .then(response => response.text())
        .then(status => {
            lastStatuses[statusCounter++ % nStatuses] = status

            const isMostLikelyOn = lastStatuses.some(x => x == 1);
            statusText.textContent = `Current Sensor Status: ${status == 1 ? 'On' : 'Off'} most likely its ${isMostLikelyOn? 'Working Fine':'Something wrong'}`;

        })
        .catch(error => {
            console.error('Error fetching data:', error);
        });
}
if(clicked)
updatePercentage();







function callAutoLevel() {
    fetch('/api/Grbl/LevelIt', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
})
        .then(response => {
            response.text();
            console.log("Leveled Successfully");
        })
        .catch(error => {
            console.error('Error fetching data:', error);
        });
}

