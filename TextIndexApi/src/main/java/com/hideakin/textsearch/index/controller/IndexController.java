package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.service.IndexService;

@RestController
public class IndexController {

	@Autowired
	private IndexService service;
	
	@RequestMapping(value="/v1/index/{group:[^0-9].*}",method=RequestMethod.GET)
	public TextDistribution[] findTextByGroup(
			@PathVariable String group,
			@RequestParam(name="text") String text,
			@RequestParam(name="option") SearchOptions option) {
    	return service.findText(group, text, option);
	}

}
