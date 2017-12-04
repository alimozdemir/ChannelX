import * as Vuex from "vuex";
import  { userController, UserState } from "./userState";

export interface State {
    counterState : UserState;
}

export const createStore = () => new Vuex.Store<State>({
    modules: {
        userController
    },
});