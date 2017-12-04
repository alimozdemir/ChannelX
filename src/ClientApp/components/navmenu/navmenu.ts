import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import moment from 'moment';

interface channelListModel {
    id: number,
    title: string,
    endTime: Date,
    popularity: number
}

@Component({
    filters: {
        duration : function(value:Date){
            return moment(value).fromNow();
        }
    }
})
export default class NavMenuComponent extends Vue {
    
    publicList: channelListModel[] = [];
    engagedList: channelListModel[] = [];

    async mounted(){
        // await this.refresh();
        var that = this;
        setTimeout(function() {
            that.refresh();
        }, 1000)
        
        var id = await setInterval(async () => { await this.refresh() }, 60000)
        localStorage.setItem('interval', id.toString())
    }

    async refresh(){
        await this.list_public();
        await this.list_engaged();
    }

    async list_public() {
        let result = await axios.get('/api/Channel/Public');

        if(result.status == 200)
        {
            let data = result.data as channelListModel[];

            this.publicList = data;
        }
    }


    async list_engaged() {
        let result = await axios.get('/api/Channel/Engaged');

        if(result.status == 200)
        {
            let data = result.data as channelListModel[];

            this.engagedList = data;
        }
    }
}
