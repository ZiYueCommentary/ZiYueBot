import DefaultTheme, {VPBadge} from 'vitepress/theme'
import {Tab, Tabs} from 'vue3-tabs-component'
import '@red-asuka/vitepress-plugin-tabs/dist/style.css'

export default {
    ...DefaultTheme,
    enhanceApp({ app }) {
        app.component('Tab', Tab)
        app.component('Tabs', Tabs)
        app.component("Badge", VPBadge)
    }
}