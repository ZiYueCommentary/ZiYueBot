import {defineConfig} from 'vitepress'
import tabsPlugin from '@red-asuka/vitepress-plugin-tabs'

// https://vitepress.dev/reference/site-config
export default defineConfig({
    title: "子悦机器",
    description: "子悦机器 (ZiYue Bot) 是一个由 子悦解说 开发的，用 C# 编写的 QQ 和 Discord 机器人。",
    markdown: {
        config: (md) => {
            tabsPlugin(md)
        },
    },
    ignoreDeadLinks: true,
    themeConfig: {
        logo: '/logo.png',
        // https://vitepress.dev/reference/default-theme-config
        nav: [
            {text: '使用', link: '/usage'},
            {text: '用户协议', link: '/tos'}
        ],

        sidebar: [
            {
                text: '',
                items: [
                    {text: '使用', link: '/usage'},
                    {text: '用户协议', link: '/tos'},
                    {text: '赞助者', link: '/sponsors'}
                ]
            },
            {
                text: '鸿蒙命令',
                link: '/harmony',
                items: [
                    {text: '关于', link: '/harmony/about'},
                    {text: '今日人品', link: '/harmony/jrrp'},
                    {text: '一言', link: '/harmony/hitokoto'},
                    {text: '评价', link: '/harmony/ask'},
                    {text: '碧蓝档案标题', link: '/harmony/balogo'},
                    {text: '毛主席语录', link: '/harmony/quotations'},
                    {text: '喜报', link: '/harmony/xibao'},
                    {text: '悲报', link: '/harmony/beibao'},
                    {
                        text: '俄罗斯轮盘',
                        link: '/harmony/revolver',
                        collapsed: true,
                        items: [
                            {text: '开始俄罗斯轮盘', link: '/harmony/revolver/start'},
                            {text: '开枪', link: '/harmony/revolver/shooting'},
                            {text: '转轮', link: '/harmony/revolver/rotating'},
                            {text: '重置俄罗斯轮盘', link: '/harmony/revolver/restart'}
                        ]
                    }
                ]
            },
            {
                text: '一般命令',
                link: '/general',
                items: [
                    {text: '帮助', link: '/general/help'},
                    {text: '表情转图片', link: '/general/picface'},
                    {text: '赢', link: '/general/win'},
                    {text: '对话', link: '/general/chat'},
                    {text: '绘画', link: '/general/draw'},
                    {
                        text: '漂流云瓶',
                        link: '/general/driftbottle/',
                        collapsed: true,
                        items: [
                            {text: '扔云瓶', link: '/general/driftbottle/throw'},
                            {text: '捞云瓶', link: '/general/driftbottle/pick'},
                            {text: '删除云瓶', link: '/general/driftbottle/remove'},
                            {text: '查看我的云瓶', link: '/general/driftbottle/list'}
                        ]
                    },
                    {
                        text: '海峡云瓶',
                        link: '/general/straitbottle/',
                        collapsed: true,
                        items: [
                            {text: '扔海峡云瓶', link: '/general/straitbottle/throw'},
                            {text: '捞海峡云瓶', link: '/general/straitbottle/pick'},
                            {text: '海峡云瓶列表', link: '/general/straitbottle/list'}
                        ]
                    },
                ]
            }
        ],

        socialLinks: [
            {icon: 'github', link: 'https://github.com/ZiYueCommentary/ZiYueBot'}
        ],

        editLink: {
            pattern: 'https://github.com/vuejs/vitepress/edit/main/docs/:path',
            text: '在 GitHub 上编辑此页面'
        },

        footer: {
            message: '使用 <a href="https://vitepress.dev/zh/">VitePress</a> 搭建',
            copyright: '<a href="https://beian.miit.gov.cn/">津ICP备2024025376号</a>'
        },

        docFooter: {
            prev: '上一页',
            next: '下一页'
        },

        outline: {
            label: '页面导航'
        },

        lastUpdated: {
            text: '最后更新于'
        },

        notFound: {
            title: '页面未找到',
            quote: '但如果你不改变方向，并且继续寻找，你可能最终会到达你所前往的地方。',
            linkLabel: '前往首页',
            linkText: '前往首页'
        },

        langMenuLabel: '多语言',
        returnToTopLabel: '回到顶部',
        sidebarMenuLabel: '菜单',
        darkModeSwitchLabel: '外观',
        lightModeSwitchTitle: '切换到浅色模式',
        darkModeSwitchTitle: '切换到深色模式',
        skipToContentLabel: '跳转到内容'
    },
    head: [['link', {rel: 'icon', href: '/logo.png'}]],
})
