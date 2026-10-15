import { createApp } from 'vue'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import App from './App.vue'

// Подключаем все компоненты Vuetify разом. Для маленькой странички так проще,
// чем настраивать автоимпорт через плагин.
const vuetify = createVuetify({ components, directives })

createApp(App).use(vuetify).mount('#app')
