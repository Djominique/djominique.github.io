const toggleKnoppen = document.querySelectorAll(".toggle-btn");

toggleKnoppen.forEach((knop) => {
  knop.addEventListener("click", () => togglePost(knop));
});

function togglePost(knop) {
  const postId = knop.getAttribute("aria-controls");
  const post = document.querySelector(`#${postId}`);
  const isOpen = knop.getAttribute("aria-expanded") === "true";

  if (isOpen) {
    knop.setAttribute("aria-expanded", "false");
    post.hidden = true;
    knop.textContent = "Lees meer";
  } else {
    knop.setAttribute("aria-expanded", "true");
    post.hidden = false;
    knop.textContent = "Verberg";
  }
}