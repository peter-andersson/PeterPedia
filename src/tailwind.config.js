module.exports = {
  content: [
    './**/*.html',
    './**/*.cshtml',
    './**/*.razor',
],
  theme: {
    extend: {},
  },
  plugins: [
    require('@tailwindcss/forms')
  ],
}
