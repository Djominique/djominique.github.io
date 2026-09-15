const h1 = document.querySelector('h1');
const p = document.querySelector('p');

h1.textContent = 'Who Am I?';
p.textContent = 'Mijn naam is Dominique, ik ben 20 jaar oud en ik woon in Den Haag.';

h1.style.color = "red";


const fruits = [
  {
    name: "Appel",
    stars: 5,
    prijs: 3,
  },
  {
    name: "Banana",
    stars: 4,
    prijs: 2,
  },
  {
    name: "Lemon",
    stars: 1,
    prijs: 1
  },
  {
        name: "Mango",
        stars: 4,
        prijs: 5,
  },
  {
        name: "Pineapple",
        stars: 4,
        prijs: 7,
  }
];

const fruitList = document.querySelector('#fruit-list');
const priceButton = document.querySelector('#button1');
const starsButton = document.querySelector('#button2');

function showFruits(fruitsToShow) {
  fruitList.replaceChildren();

  fruitsToShow.forEach((fruittype) => {
  const fruitDiv = document.createElement('div');
  fruitDiv.classList.add('fruit');
  fruitDiv.innerHTML = `
    <h2>${fruittype.name}</h2>
    <p>Prijs: €${fruittype.prijs.toFixed(2)}</p>
    <p>Stars: ${'★'.repeat(fruittype.stars)}</p>
  `;
    fruitList.appendChild(fruitDiv);
  });
}

priceButton.addEventListener('click', () => {
  showFruits([...fruits].sort((secondFruit, firstFruit) => firstFruit.prijs - secondFruit.prijs));
});

starsButton.addEventListener('click', () => {
  showFruits([...fruits].sort((secondFruit, firstFruit) => firstFruit.stars - secondFruit.stars));
});

showFruits(fruits);

