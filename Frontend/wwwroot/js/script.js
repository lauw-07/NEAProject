document.addEventListener("DOMContentLoaded", () => {
    const optionMenu = document.getElementById("dropdown-menu");
    const selectBtn = optionMenu.querySelector(".select-btn");
    const options = optionMenu.querySelectorAll(".option");
    const selectBtn_text = optionMenu.querySelector(".selectBtn-text");

    selectBtn.addEventListener("click", () => {
        optionMenu.classList.toggle("active");
    });

    options.forEach(option => {
        option.addEventListener("click", () => {
            let selectedOption = option.querySelector(".option-text").innerText;
            selectBtn_text.innerText = selectedOption;
            optionMenu.classList.remove("active");
        });
    });
});
