function toonWeer() {
  const status = document.querySelector("#weer-status");
  status.textContent = "Locatie ophalen...";

  if (!navigator.geolocation) {
    status.textContent = "Locatie wordt niet ondersteund door je browser.";
    return;
  }

  navigator.geolocation.getCurrentPosition(
    (positie) => haalWeerOp(positie.coords.latitude, positie.coords.longitude),
    () => {
      status.textContent = "Geen toestemming voor locatie — weer kan niet worden getoond.";
    }
  );
}

async function haalWeerOp(latitude, longitude) {
  const status = document.querySelector("#weer-status");
  status.textContent = "Weer ophalen...";
  try {
    const response = await fetch(
      `https://api.open-meteo.com/v1/forecast?latitude=${latitude}&longitude=${longitude}&current_weather=true`
    );

    if (!response.ok) {
      throw new Error("API niet bereikbaar");
    }

    const data = await response.json();
    const temperatuur = data.current_weather.temperature;

    status.textContent = `${temperatuur}°C bij jou in de buurt`;
  } catch (error) {
    status.textContent = "Weer kon niet worden geladen.";
  }
}

toonWeer();