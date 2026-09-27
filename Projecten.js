const projecten = [
  {
    titel: "Project 1",
    semester: "1",
    beschrijving:
      "In semester 1 heb ik met mijn groepje een app ontwikkeld waarmee je watertappen in jouw omgeving kunt vinden en de status daarvan kunt bekijken. Ook kon je de app gebruiken om een watertappunt aan te vragen bij jou in de buurt. Ik heb bij het maken van dit project vooral de backend kant van de app ontwikkeld. Hier heb ik Java voor gebruikt en heb ik de app gekoppeld aan een database. Ook heb ik de app getest en verbeterd waar nodig.",
  },
  {
    titel: "Project 2",
    semester: "2",
    beschrijving:
      "In semester 2 heb ik met mijn groepje een hotelsimulatie ontwikkeld. Hierin kwamen er klanten binnen die vervolgens een kamer toegewezen kregen. Ook vonden er verschillende events plaats zoals het inchecken van een klant, het schoonmaken van een kamer en het uitruimen van de kamers door een brandalarm. Ik heb bij het maken van dit project vooral de backend kant van de app ontwikkeld. Hier heb ik Java voor gebruikt en heb ik de app gekoppeld aan een database. Ook heb ik de app getest en verbeterd waar nodig.",
  },
];

renderProjecten(projecten);

function renderProjecten(lijst) {
  const container = document.querySelector("#projecten");

  container.querySelectorAll("article").forEach((article) => article.remove());

  lijst.forEach((project) => {
    const article = document.createElement("article");

    const h2 = document.createElement("h2");
    h2.textContent = project.titel;

    const p = document.createElement("p");
    p.textContent = project.beschrijving;

    article.append(h2, p);
    container.appendChild(article);
  });
}
const filterSelect = document.querySelector("#semester-filter");

filterSelect.addEventListener("change", () => {
  const gekozenSemester = filterSelect.value;

  let gefilterd;

  if (gekozenSemester === "alle") {
    gefilterd = projecten;
  } else {
    gefilterd = projecten.filter(
      (project) => project.semester === gekozenSemester,
    );
  }
  renderProjecten(gefilterd);
});
