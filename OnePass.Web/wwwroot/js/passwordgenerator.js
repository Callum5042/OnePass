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

        // Clamp min/max length
        this.minLengthRange.addEventListener("input", () => this.clampMinLength());
        this.minLengthEl.addEventListener("input", () => this.clampMinLength());
        this.maxLengthRange.addEventListener("input", () => this.clampMaxLength());
        this.maxLengthEl.addEventListener("input", () => this.clampMaxLength());

        // Auto-submit when changing a value
        this.amountRange.addEventListener("input", () => this.generatePassword());
        this.amountEl.addEventListener("input", () => this.generatePassword());
        this.minLengthRange.addEventListener("input", () => this.generatePassword());
        this.minLengthEl.addEventListener("input", () => this.generatePassword());
        this.maxLengthRange.addEventListener("input", () => this.generatePassword());
        this.maxLengthEl.addEventListener("input", () => this.generatePassword());

        // Restrict turning off all combinations
        this.uppercaseEl.addEventListener("input", async (e) => await this.checkCombinationBeforeSubmit(e));
        this.lowercaseEl.addEventListener("input", async (e) => await this.checkCombinationBeforeSubmit(e));
        this.numbersEl.addEventListener("input", async (e) => await this.checkCombinationBeforeSubmit(e));
        this.symbolsEl.addEventListener("input", async (e) => await this.checkCombinationBeforeSubmit(e));
    }

    clampMinLength() {
        if (Number(this.minLengthEl.value) > Number(this.maxLengthEl.value)) {
            this.maxLengthEl.value = this.minLengthEl.value;
        }

        if (Number(this.minLengthRange.value) > Number(this.maxLengthRange.value)) {
            this.maxLengthRange.value = this.minLengthRange.value;
        }
    }

    clampMaxLength() {
        if (Number(this.maxLengthEl.value) < Number(this.minLengthEl.value)) {
            this.minLengthEl.value = this.maxLengthEl.value;
        }

        if (Number(this.maxLengthRange.value) < Number(this.minLengthRange.value)) {
            this.minLengthRange.value = this.maxLengthRange.value;
        }
    }

    async checkCombinationBeforeSubmit(e) {

        let activeCount = 0;

        if (this.uppercaseEl.checked) {
            activeCount++;
        }

        if (this.lowercaseEl.checked) {
            activeCount++;
        }

        if (this.numbersEl.checked) {
            activeCount++;
        }

        if (this.symbolsEl.checked) {
            activeCount++;
        }

        if (activeCount === 0) {
            e.target.checked = true;
        }
        else {
            await this.generatePassword();
        }
    }

    async generatePassword() {

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

        // Push URL
        const windowUrl = `/generate-password?${searchParams.toString()}`;
        window.history.pushState({}, null, windowUrl);

        // HTTP Request
        const response = await fetch(url);
        const data = await response.json();

        // Output to textarea
        const text = data.map(x => x.password).join("\n");
        const textarea = document.getElementById("Passwords");
        textarea.value = text;
    }
}