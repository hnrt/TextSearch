package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.service.FileGroupService;
import com.hideakin.textsearch.index.service.IndexService;

@RestController
public class IndexController {

	@Autowired
	private IndexService indexService;

	@Autowired
	private FileGroupService fileGroupService;

	@RequestMapping(value="/v1/index/{group:[^0-9].*}/{text}",method=RequestMethod.GET)
	public ResponseEntity<?> findText(
			@PathVariable String group,
			@PathVariable String text,
			@RequestParam(name="option") SearchOptions option,
			@RequestParam(name="limit") int limit,
			@RequestParam(name="offset") int offset) {
		FileGroupInfo info = fileGroupService.getGroup(group);
		if (info != null) {
			TextDistribution[] results = indexService.find(info.getGid(), text, option, limit, offset);
			return new ResponseEntity<>(results, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/index/{group:[^0-9].*}/{text}",method=RequestMethod.POST)
	public ResponseEntity<?> postText(
			@PathVariable String group,
			@PathVariable String text,
			@RequestBody TextDistribution[] data) {
		FileGroupInfo info = fileGroupService.getGroup(group);
		if (info != null) {
			indexService.add(text, info.getGid(), data);
			return new ResponseEntity<>(HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

}
