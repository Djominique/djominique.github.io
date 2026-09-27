const form = document.querySelector("#contact-form");
const formStatus = document.querySelector("#form-status");

const velden = [
  {
    id: "naam",
    errorId: "naam-error",
    valideer: (waarde) => waarde.trim() !== "",
    foutmelding: "Vul je naam in.",
  },
  {
    id: "email",
    errorId: "email-error",
    valideer: (waarde) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(waarde.trim()),
    foutmelding: "Vul een geldig e-mailadres in.",
  },
  {
    id: "bericht",
    errorId: "bericht-error",
    valideer: (waarde) => waarde.trim() !== "",
    foutmelding: "Vul een bericht in.",
  },
];

function valideerVeld(veld) {
  const input = document.querySelector(`#${veld.id}`);
  const foutmeldingEl = document.querySelector(`#${veld.errorId}`);

  if (!veld.valideer(input.value)) {
    foutmeldingEl.textContent = veld.foutmelding;
    return false;
  }

  foutmeldingEl.textContent = "";
  return true;
}

form.addEventListener("submit", (event) => {
  event.preventDefault();

  const resultaten = velden.map(valideerVeld);
  const allesGeldig = resultaten.every((geldig) => geldig);

  if (allesGeldig) {
    formStatus.textContent = "Bedankt! Je bericht is verstuurd.";
    form.reset();
  } else {
    formStatus.textContent = "Controleer de gemarkeerde velden hierboven.";
  }
});