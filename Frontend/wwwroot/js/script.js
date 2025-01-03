function InitialiseDropdownMenus() {
    const optionMenus = document.querySelectorAll(".dropdown-menu");

    optionMenus.forEach(optionMenu => {
        const selectBtn = optionMenu.querySelector(".select-btn");
        const options = optionMenu.querySelectorAll(".option");
        const selectBtn_text = optionMenu.querySelector(".selectBtn-text");

        selectBtn.addEventListener("click", () => {
            optionMenu.classList.toggle("active");
        });


    });
};
