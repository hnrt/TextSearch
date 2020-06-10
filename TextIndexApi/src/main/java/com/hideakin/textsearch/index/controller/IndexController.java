package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.FindTextResponse;
import com.hideakin.textsearch.index.model.UpdateIndexRequest;
import com.hideakin.textsearch.index.model.UpdateIndexResponse;
import com.hideakin.textsearch.index.service.IndexService;

@RestController
public class IndexController {

	@Autowired
	private IndexService service;
	
	@RequestMapping(value="/v1/index",method=RequestMethod.GET)
	public FindTextResponse findText(
			@RequestParam(name = "text") String text,
			@RequestParam(name = "option", defaultValue = "exact") SearchOptions option) {
    	return service.findText("default", text, option);
	}

	@RequestMapping(value="/v1/index",method=RequestMethod.POST)
	public UpdateIndexResponse updateIndex(
			@RequestBody UpdateIndexRequest req) {
		return service.updateIndex("default", req);
	}

	@RequestMapping(value="/v1/index",method=RequestMethod.DELETE)
	public void deleteIndex() {
		service.deleteIndex("default");
	}

	@RequestMapping(value="/v1/index/{group}",method=RequestMethod.GET)
	public FindTextResponse findTextByGroup(
			@PathVariable String group,
			@RequestParam(name = "text") String text,
			@RequestParam(name = "option", defaultValue = "exact") SearchOptions option) {
    	return service.findText(group, text, option);
	}

	@RequestMapping(value="/v1/index/{group}",method=RequestMethod.POST)
	public UpdateIndexResponse updateIndexByGroup(
			@PathVariable String group,
			@RequestBody UpdateIndexRequest req) {
		return service.updateIndex(group, req);
	}

	@RequestMapping(value="/v1/index/{group}",method=RequestMethod.DELETE)
	public void deleteIndexByGroup(
			@PathVariable String group) {
		service.deleteIndex(group);
	}

}
