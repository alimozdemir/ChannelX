import Vue from 'vue';
import './historypage.css'
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import swal from 'sweetalert';
import resultModel from '../../models/resultModel';
import moment from 'moment';


interface historyModel {
    id: number,
    title: string,
    endAt: Date,
    createdAt: Date,
    engagedUsersName: string[]
}

interface historyPaginationModel {
    total: number,
    currentPage: number,
    count: number
}

@Component({
    filters: {
        duration: function (model: historyModel) {
            let end = moment(model.endAt);
            let diff = end.diff(model.createdAt);
            return moment.duration(diff).asHours();
        },
        endTime: function (value: Date) {
            let end = moment(value).format('MMMM Do YYYY, h:mm:ss a');
            return end;
        }
    }
})
export default class HistoryPageComponent extends Vue {

    modelList: historyModel[] = [];
    counts: number[] = [5, 10, 15, 20];
    page: historyPaginationModel = { total: 0, count: this.counts[0], currentPage: 1 }
    max: number = 0;
    loading: boolean = true;
    pages: number[] = [];
    async mounted() {
        setTimeout(async () => {

            await this.initial();
        }, 200)
    }
    async initial() {
        let result = await axios.get('/api/Channel/HistoryPageTotal')
        if (result.status == 200) {
            this.page.total = result.data as number;
            await this.get();
        }
        else {
            swal({ text: "something went wrong", icon: "error" })
        }

    }
    async get() {
        this.loading = true;
        let result = await axios.get('/api/Channel/HistoryPage', {
            params: this.page
        });

        if (result.status == 200) {
            let data = result.data as historyModel[];
            this.modelList = data;
            this.calculatePages();
            this.loading = false;
        }
        else {

            swal({ text: "something went wrong", icon: "error" })
        }
    }

    calculatePages(){
        let array: number[] = [];
        this.max = this.page.total / this.page.count;
        
        for(let i = 0; i < this.max; i++)
            array.push(i + 1);
        this.pages = array;
        if(this.max > 0)
        this.page.currentPage = array[0];
    }

}