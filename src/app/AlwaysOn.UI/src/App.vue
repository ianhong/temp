<template>
    <div id="header" class="c1000">
        <a class="header-logo" href="https://www.costco.com/" id="WC_CachedHeaderDisplay_Link_2a"><img src="/img/logo.png" alt="USBC" height="75px"></a>
    </div>
    <div>
        <router-link :to="{ path: '/' }">Home</router-link> |
        <router-link :to="{ name: 'catalogPage' }">Catalog</router-link>

        <hr />

        <SystemMessage />

        <router-view :key="$route.fullPath"></router-view>

        <hr />

        <footer class="footer">
            <div id="footer-bottom" class="footer_copyright">
                <p class="copyright">&#169; 1998 &#8212; <span id="copyright-year">2023</span> Costco Wholesale Corporation. <span class="rights">All rights reserved.</span></p>
            </div>
        </footer>
    </div>
</template>

<script>
import SystemMessage from "./components/SystemMessage";

export default {
  data: function() {
     return {
        versionLabel: window.VERSION_LABEL
      }
  },
  created() { },
  methods: { },
  computed: { },
  components: {
    SystemMessage,
  },
};
</script>
