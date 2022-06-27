class PasswordGenerator {

    constructor(url) {
        this.baseUrl = url;

        // Cache elements
        this.amountEl = document.getElementById("Amount");
        this.minLengthEl = document.getElementById("MinLength");
        this.maxLengthEl = document.getElementById("MaxLength");
        this.uppercaseEl = document.getElementById("uppercase-switch");
        this.lowercaseEl = document.getElementById("lowercase-switch");
        this.numbersEl = document.getElementById("numbers-switch");
        this.symbolsEl = document.getElementById("symbols-switch");

        this.amountRange = document.getElementById("amount-range");
        this.minLengthRange = document.getElementById("minlength-range");
        this.maxLengthRange = document.getElementById("maxlength-range");

        // Range sliders
        this.amountRange.addEventListener("input", (e) => this.amountEl.value = e.target.value);
        this.amountEl.addEventListener("input", (e) => this.amountRange.value = e.target.value);

        this.minLengthRange.addEventListener("input", (e) => this.minLengthEl.value = e.target.value);
        this.minLengthEl.addEventListener("input", (e) => this.minLengthRange.value = e.target.value);

        this.maxLengthRange.addEventListener("input", (e) => this.maxLengthEl.value = e.target.value);
        this.maxLengthEl.addEventListener("input", (e) => this.maxLengthRange.value = e.target.value);

        // Form submit
        const form = document.getElementById("generate-password-form");
        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            await this.generatePassword();
        });

        // Auto-submit when changing a value
        this.amountRange.addEventListener("input", () => this.generatePassword());
        this.amountEl.addEventListener("input", () => this.generatePassword());
        this.minLengthRange.addEventListener("input", () => this.generatePassword());
        this.minLengthEl.addEventListener("input", () => this.generatePassword());
        this.maxLengthRange.addEventListener("input", () => this.generatePassword());
        this.maxLengthEl.addEventListener("input", () => this.generatePassword());

        this.uppercaseEl.addEventListener("input", () => this.generatePassword());
        this.lowercaseEl.addEventListener("input", () => this.generatePassword());
        this.numbersEl.addEventListener("input", () => this.generatePassword());
        this.symbolsEl.addEventListener("input", () => this.generatePassword());
    }

    async generatePassword() {

        console.log("Submit");

        // Build URL
        const searchParams = new URLSearchParams();
        searchParams.set("Amount", this.amountEl.value);
        searchParams.set("MinLength", this.minLengthEl.value);
        searchParams.set("MaxLength", this.maxLengthEl.value);
        searchParams.set("Uppercase", this.uppercaseEl.checked);
        searchParams.set("Lowercase", this.lowercaseEl.checked);
        searchParams.set("Numbers", this.numbersEl.checked);
        searchParams.set("Symbols", this.symbolsEl.checked);
        const url = `${this.baseUrl}?${searchParams.toString()}`;

        // HTTP Request
        const response = await fetch(url);
        const data = await response.json();

        // Output to textarea
        const text = data.map(x => x.password).join("\n");
        const textarea = document.getElementById("Passwords");
        textarea.value = text;
    }
}