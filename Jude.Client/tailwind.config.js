import { heroui } from "@heroui/react";

export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
    "./node_modules/@heroui/theme/dist/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        geist: ["Geist", "sans-serif"],
      },
    },
  },
  darkMode: "class",
  plugins: [
    heroui({
      layout: {
        dividerWeight: "1px", 
        disabledOpacity: 0.45, 
        fontSize: {
          tiny: "0.75rem",   // 12px
          small: "0.875rem", // 14px
          medium: "0.9375rem", // 15px
          large: "1.125rem", // 18px
        },
        lineHeight: {
          tiny: "1rem", 
          small: "1.25rem", 
          medium: "1.5rem", 
          large: "1.75rem", 
        },
        radius: {
          small: "6px", 
          medium: "8px", 
          large: "12px", 
        },
        borderWidth: {
          small: "1px", 
          medium: "1px", 
          large: "2px", 
        },
      },
      themes: {
        light: {
          colors: {
            "background": {
              "DEFAULT": "#FFFFFF"
            },
            "content1": {
              "DEFAULT": "#FFFFFF",
              "foreground": "#11181C"
            },
            "content2": {
              "DEFAULT": "#f4f4f5",
              "foreground": "#27272a"
            },
            "content3": {
              "DEFAULT": "#e4e4e7",
              "foreground": "#3f3f46"
            },
            "content4": {
              "DEFAULT": "#d4d4d8",
              "foreground": "#52525b"
            },
            "divider": {
              "DEFAULT": "rgba(17, 17, 17, 0.15)"
            },
            "focus": {
              "DEFAULT": "#00a5cf"
            },
            "foreground": {
              "50": "#fafafa",
              "100": "#f4f4f5",
              "200": "#e4e4e7",
              "300": "#d4d4d8",
              "400": "#a1a1aa",
              "500": "#71717a",
              "600": "#52525b",
              "700": "#3f3f46",
              "800": "#27272a",
              "900": "#18181b",
              "DEFAULT": "#11181C"
            },
            "overlay": {
              "DEFAULT": "#000000"
            },
            "danger": {
              "50": "#fee7ef",
              "100": "#fdd0df",
              "200": "#faa0bf",
              "300": "#f871a0",
              "400": "#f54180",
              "500": "#f31260",
              "600": "#c20e4d",
              "700": "#920b3a",
              "800": "#610726",
              "900": "#310413",
              "DEFAULT": "#f31260",
              "foreground": "#ffffff"
            },
            "default": {
              "50": "#fafafa",
              "100": "#f4f4f5",
              "200": "#e4e4e7",
              "300": "#d4d4d8",
              "400": "#a1a1aa",
              "500": "#71717a",
              "600": "#52525b",
              "700": "#3f3f46",
              "800": "#27272a",
              "900": "#18181b",
              "DEFAULT": "#d4d4d8",
              "foreground": "#000"
            },
            "primary": {
              "50": "#e6f5f8",
              "100": "#ccebf2",
              "200": "#99d7e5",
              "300": "#66c3d8",
              "400": "#33afcb",
              "500": "#00a5cf",
              "600": "#0084a6",
              "700": "#00637c",
              "800": "#004253",
              "900": "#002129",
              "DEFAULT": "#00a5cf",
              "foreground": "#fff"
            },
            "secondary": {
              "50": "#e6f0f1",
              "100": "#cce1e3",
              "200": "#99c3c7",
              "300": "#66a5ab",
              "400": "#33878f",
              "500": "#25a18e",
              "600": "#1e8172",
              "700": "#166155",
              "800": "#0f4039",
              "900": "#07201c",
              "DEFAULT": "#25a18e",
              "foreground": "#fff"
            },
            "success": {
              "50": "#ecfaed",
              "100": "#d9f5db",
              "200": "#b3ebb7",
              "300": "#8de093",
              "400": "#7ae582",
              "500": "#7ae582",
              "600": "#4eb957",
              "700": "#3a8b41",
              "800": "#275c2c",
              "900": "#132e16",
              "DEFAULT": "#7ae582",
              "foreground": "#000"
            },
            "warning": {
              "50": "#fefce8",
              "100": "#fdedd3",
              "200": "#fbdba7",
              "300": "#f9c97c",
              "400": "#f7b750",
              "500": "#f5a524",
              "600": "#c4841d",
              "700": "#936316",
              "800": "#62420e",
              "900": "#312107",
              "DEFAULT": "#f5a524",
              "foreground": "#000"
            }
          }
        },
        dark: {
          colors: {
            "background": {
              "DEFAULT": "#004e64"
            },
            "content1": {
              "DEFAULT": "#003a4b",
              "foreground": "#f0f9fc"
            },
            "content2": {
              "DEFAULT": "#003342",
              "foreground": "#e0f5fa"
            },
            "content3": {
              "DEFAULT": "#002c39",
              "foreground": "#d0f1f8"
            },
            "content4": {
              "DEFAULT": "#002530",
              "foreground": "#c0edf6"
            },
            "divider": {
              "DEFAULT": "rgba(255, 255, 255, 0.15)"
            },
            "focus": {
              "DEFAULT": "#00a5cf"
            },
            "foreground": {
              "50": "#002530",
              "100": "#002c39",
              "200": "#003342",
              "300": "#003a4b",
              "400": "#004e64",
              "500": "#006a88",
              "600": "#0086ac",
              "700": "#00a5cf",
              "800": "#33b5d8",
              "900": "#66c5e1",
              "DEFAULT": "#f0f9fc"
            },
            "overlay": {
              "DEFAULT": "#000000"
            },
            "danger": {
              "50": "#310413",
              "100": "#610726",
              "200": "#920b3a",
              "300": "#c20e4d",
              "400": "#f31260",
              "500": "#f54180",
              "600": "#f871a0",
              "700": "#faa0bf",
              "800": "#fdd0df",
              "900": "#fee7ef",
              "DEFAULT": "#f31260",
              "foreground": "#ffffff"
            },
            "default": {
              "50": "#002530",
              "100": "#002c39",
              "200": "#003342",
              "300": "#003a4b",
              "400": "#004e64",
              "500": "#006a88",
              "600": "#0086ac",
              "700": "#00a5cf",
              "800": "#33b5d8",
              "900": "#66c5e1",
              "DEFAULT": "#004e64",
              "foreground": "#fff"
            },
            "primary": {
              "50": "#002129",
              "100": "#004253",
              "200": "#00637c",
              "300": "#0084a6",
              "400": "#00a5cf",
              "500": "#33b5d8",
              "600": "#66c5e1",
              "700": "#99d7e5",
              "800": "#ccebf2",
              "900": "#e6f5f8",
              "DEFAULT": "#00a5cf",
              "foreground": "#fff"
            },
            "secondary": {
              "50": "#07201c",
              "100": "#0f4039",
              "200": "#166155",
              "300": "#1e8172",
              "400": "#25a18e",
              "500": "#51b4a5",
              "600": "#7dc7bc",
              "700": "#a8dad3",
              "800": "#d4ece9",
              "900": "#e9f6f4",
              "DEFAULT": "#25a18e",
              "foreground": "#fff"
            },
            "success": {
              "50": "#132e16",
              "100": "#275c2c",
              "200": "#3a8b41",
              "300": "#4eb957",
              "400": "#7ae582",
              "500": "#92ea9a",
              "600": "#abefb1",
              "700": "#c3f4c8",
              "800": "#e1fae4",
              "900": "#f0fcf1",
              "DEFAULT": "#7ae582",
              "foreground": "#000"
            },
            "warning": {
              "50": "#312107",
              "100": "#62420e",
              "200": "#936316",
              "300": "#c4841d",
              "400": "#f5a524",
              "500": "#f7b750",
              "600": "#f9c97c",
              "700": "#fbdba7",
              "800": "#fdedd3",
              "900": "#fefce8",
              "DEFAULT": "#f5a524",
              "foreground": "#000"
            }
          }
        }
      }
    })
    ,
  require("@tailwindcss/typography"),
  require("tailwindcss-animate"),
],
}

